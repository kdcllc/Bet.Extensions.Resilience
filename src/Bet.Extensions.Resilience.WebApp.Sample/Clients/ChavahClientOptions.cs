﻿using Bet.Extensions.Resilience.Http.Abstractions.Options;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients
{
    public class ChavahClientOptions : HttpClientOptions
    {
        public string SomeValue { get; set; }
    }
}
