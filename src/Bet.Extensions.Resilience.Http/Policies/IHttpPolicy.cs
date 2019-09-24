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
        /// The method to register the policies with the.
        /// </summary>
        void RegisterPolicy();

        IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy();

        ISyncPolicy<HttpResponseMessage> CreateSyncPolicy();
    }
}
