using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The default interface that allows for Policy Configurations framework to configure it.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IHttpPolicy<TOptions> where TOptions : HttpPolicyOptions
    {
        /// <summary>
        /// The name of the Http Policy to be configured.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Policies options.
        /// </summary>
        TOptions Options { get; }

        /// <summary>
        /// The method to register the policies with the.
        /// </summary>
        void RegisterPolicy();

        /// <summary>
        /// Create Async Polly Policy for <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy();

        /// <summary>
        /// Create Sync Polly Policy for <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <returns></returns>
        ISyncPolicy<HttpResponseMessage> CreateSyncPolicy();
    }
}
