namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// Polly context helper.
    /// </summary>
    public static class PolicyContextItems
    {
        public const string Logger = nameof(Logger);

        public const string HttpClientName = nameof(HttpClientName);
    }
}
