﻿using System.Net.Http;

namespace Bet.Extensions.Resilience.UnitTest
{
    // Simple typed client for use in tests
    public class TestTypedClient : ITestTypedClient
    {
        public TestTypedClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }
}