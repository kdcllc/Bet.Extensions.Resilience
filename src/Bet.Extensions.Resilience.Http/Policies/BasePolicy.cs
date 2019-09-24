using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <inheritdoc/>
    public abstract class BasePolicy<TOptions> : IHttpPolicy<TOptions> where TOptions : HttpPolicyOptions
    {
        private readonly IHttpPolicyConfigurator<TOptions> _policyConfigurator;

        protected BasePolicy(
            string policyName,
            IHttpPolicyConfigurator<TOptions> policyConfigurator,
            ILogger<IHttpPolicy<TOptions>> logger)
        {
            Name = policyName;

            _policyConfigurator = policyConfigurator ?? throw new ArgumentNullException(nameof(policyConfigurator));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Options = policyConfigurator.GetOptions(policyName);
        }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <inheritdoc/>
        public virtual TOptions Options { get; }

        protected ILogger<IHttpPolicy<TOptions>> Logger { get; }

        /// <inheritdoc/>
        public abstract IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy();

        /// <inheritdoc/>
        public abstract ISyncPolicy<HttpResponseMessage> CreateSyncPolicy();

        /// <inheritdoc/>
        public virtual void RegisterPolicy()
        {
            _policyConfigurator.AddPolicy($"{Name}Async", CreateAsyncPolicy, true);
            _policyConfigurator.AddPolicy($"{Name}", CreateSyncPolicy, true);
        }
    }
}
