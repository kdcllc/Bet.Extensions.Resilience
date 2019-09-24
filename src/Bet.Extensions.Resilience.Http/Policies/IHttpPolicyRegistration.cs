﻿using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public interface IHttpPolicyRegistration<TOptions> where TOptions : HttpPolicyOptions
    {
        string Name { get; }

        void RegisterPolicy();

        IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy();

        ISyncPolicy<HttpResponseMessage> CreateSyncPolicy();
    }
}
