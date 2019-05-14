using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.MessageHandlers.PollyHttp
{
    /// <summary>
    /// Based on https://github.com/aspnet/Extensions/blob/e190dcd860fd16af2403190c480b112744fa5a9d/src/HttpClientFactory/Polly/src/PolicyHttpMessageHandler.cs
    /// In addition to the regular Polly Http Message Handler this one allows for logging of the Polly execution context.
    /// A <see cref="DelegatingHandler"/> implementation that executes request processing surrounded by a <see cref="Policy"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This message handler implementation supports the use of policies provided by the Polly library for
    /// transient-fault-handling and resiliency.
    /// </para>
    /// <para>
    /// The documentation provided here is focused guidance for using Polly together with the <see cref="IHttpClientFactory"/>.
    /// See the Polly project and its documentation (https://github.com/app-vnext/Polly) for authoritative information on Polly.
    /// </para>
    /// <para>
    /// The extension methods on <see cref="PollyHttpClientBuilderExtensions"/> are designed as a convenient and correct
    /// way to create a <see cref="PolicyHttpMessageHandler"/>.
    /// </para>
    /// <para>
    /// The <see cref="PollyHttpClientBuilderExtensions.AddPolicyHandler(IHttpClientBuilder, IAsyncPolicy{HttpResponseMessage})"/>
    /// method supports the creation of a <see cref="PolicyHttpMessageHandler"/> for any kind of policy. This includes
    /// non-reactive policies, such as Timeout or Cache, which don't require the underlying request to fail first.
    /// </para>
    /// <para>
    /// <see cref="PolicyHttpMessageHandler"/> and the <see cref="PollyHttpClientBuilderExtensions"/> convenience methods
    /// only accept the generic <see cref="IAsyncPolicy{HttpResponseMessage}"/>. Generic policy instances can be created
    /// by using the generic methods on <see cref="Policy"/> such as <see cref="Policy.TimeoutAsync{TResult}(int)"/>.
    /// </para>
    /// <para>
    /// To adapt an existing non-generic <see cref="IAsyncPolicy"/>, use code like the following:
    /// <example>
    /// Converting a non-generic <code>IAsyncPolicy policy</code> to <see cref="IAsyncPolicy{HttpResponseMessage}"/>.
    /// <code>
    /// policy.AsAsyncPolicy&lt;HttpResponseMessage&gt;()
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// The <see cref="PollyHttpClientBuilderExtensions.AddTransientHttpErrorPolicy(IHttpClientBuilder, Func{PolicyBuilder{HttpResponseMessage}, IAsyncPolicy{HttpResponseMessage}})"/>
    /// method is an opinionated convenience method that supports the application of a policy for requests that fail due
    /// to a connection failure or server error (5XX HTTP status code). This kind of method supports only reactive policies
    /// such as Retry, Circuit-Breaker or Fallback. This method is only provided for convenience; we recommend creating
    /// your own policies as needed if this does not meet your requirements.
    /// </para>
    /// <para>
    /// Take care when using policies such as Retry or Timeout together as HttpClient provides its own timeout via
    /// <see cref="HttpClient.Timeout"/>.  When combining Retry and Timeout, <see cref="HttpClient.Timeout"/> will act as a
    /// timeout across all tries; a Polly Timeout policy can be configured after a Retry policy in the configuration sequence,
    /// to provide a timeout-per-try.
    /// </para>
    /// <para>
    /// All policies provided by Polly are designed to be efficient when used in a long-lived way. Certain policies such as the
    /// Bulkhead and Circuit-Breaker maintain state and should be scoped across calls you wish to share the Bulkhead or Circuit-Breaker state.
    /// Take care to ensure the correct lifetimes when using policies and message handlers together in custom scenarios. The extension
    /// methods provided by <see cref="PollyHttpClientBuilderExtensions"/> are designed to assign a long lifetime to policies
    /// and ensure that they can be used when the handler rotation feature is active.
    /// </para>
    /// <para>
    /// The <see cref="PolicyHttpMessageHandler"/> will attach a context to the <see cref="HttpRequestMessage"/> prior
    /// to executing a <see cref="Policy"/>, if one does not already exist. The <see cref="Context"/> will be provided
    /// to the policy for use inside the <see cref="Policy"/> and in other message handlers.
    /// </para>
    /// </remarks>
    public class PolicyWithLoggingHttpMessageHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        private readonly ILogger _logger;
        private readonly string _typedClientName;
        private readonly Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> _policySelector;

        /// <summary>
        /// Creates a new <see cref="PolicyWithLoggingHttpMessageHandler"/>.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="logger">The logger to be used for Polly context.</param>
        /// <param name="typedClientName">The name of the typed HttpClient to be used for the logging.</param>
        public PolicyWithLoggingHttpMessageHandler(
            IAsyncPolicy<HttpResponseMessage> policy,
            ILogger logger,
            string typedClientName)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _typedClientName = typedClientName;
        }

        /// <summary>
        /// Creates a new <see cref="PolicyWithLoggingHttpMessageHandler"/>.
        /// </summary>
        /// <param name="policySelector">A function which can select the desired policy for a given <see cref="HttpRequestMessage"/>.</param>
        /// <param name="logger">The logger to be used for Polly context.</param>
        /// <param name="typedClientName">The name of the typed HttpClient to be used for the logging.</param>
        public PolicyWithLoggingHttpMessageHandler(
            Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policySelector,
            ILogger logger,
            string typedClientName)
        {
            _policySelector = policySelector ?? throw new ArgumentNullException(nameof(policySelector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _typedClientName = typedClientName;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Guarantee the existence of a context for every policy execution, but only create a new one if needed. This
            // allows later handlers to flow state if desired.
            var cleanUpContext = false;
            var context = request.GetPolicyExecutionContext();
            if (context == null)
            {
                context = new Context();
                request.SetPolicyExecutionContext(context);
                cleanUpContext = true;

                // register the logger to be used
                context.AddTypedHttpClientLogger(_logger, _typedClientName);
            }

            HttpResponseMessage response;
            try
            {
                var policy = _policy ?? SelectPolicy(request);
                response = await policy.ExecuteAsync((c, ct) => SendCoreAsync(request, c, ct), context, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (cleanUpContext)
                {
                    request.SetPolicyExecutionContext(null);
                }
            }

            return response;
        }

        /// <summary>
        /// Called inside the execution of the <see cref="Policy"/> to perform request processing.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        /// <param name="context">The <see cref="Context"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>Returns a <see cref="Task{HttpResponseMessage}"/> that will yield a response when completed.</returns>
        protected virtual Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, Context context, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return base.SendAsync(request, cancellationToken);
        }

        private IAsyncPolicy<HttpResponseMessage> SelectPolicy(HttpRequestMessage request)
        {
            var policy = _policySelector(request);
            if (policy == null)
            {
                var message = "The 'policySelector' function must return a non-null policy instance. " +
                    "To create a policy that takes no action, use 'Policy.NoOpAsync<HttpResponseMessage>()'.";
                throw new InvalidOperationException(message);
            }

            return policy;
        }
    }
}
