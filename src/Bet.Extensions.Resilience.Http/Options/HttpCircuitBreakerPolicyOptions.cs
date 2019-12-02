using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpCircuitBreakerPolicyOptions : CircuitBreakerPolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(HttpCircuitBreakerPolicyOptions).Substring(0, nameof(HttpCircuitBreakerPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;
    }
}
