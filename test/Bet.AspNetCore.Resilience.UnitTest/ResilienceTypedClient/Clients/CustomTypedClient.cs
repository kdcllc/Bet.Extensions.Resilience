﻿namespace Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients;

// Simple typed client for use in tests
public class CustomTypedClient : ICustomTypedClient
{
    public CustomTypedClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }
}
