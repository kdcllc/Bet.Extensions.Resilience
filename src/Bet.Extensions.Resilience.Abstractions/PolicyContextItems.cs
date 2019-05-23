using System.Net.Http;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// Polly context helper.
    /// </summary>
    public static class PolicyContextItems
    {
        /// <summary>
        /// Name of the Logger to be used with Polly Context.
        /// </summary>
        public const string Logger = nameof(Logger);

        /// <summary>
        /// Used with Polly Context logger with <see cref="HttpClient"/>.
        /// </summary>
        public const string HttpClientName = nameof(HttpClientName);

        /// <summary>
        /// Used with Polly Context Logger to get the name of the executed action.
        /// </summary>
        public const string ActionName = nameof(ActionName);
    }
}
