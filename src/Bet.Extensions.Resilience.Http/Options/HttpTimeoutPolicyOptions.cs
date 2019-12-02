using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpTimeoutPolicyOptions : TimeoutPolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(HttpTimeoutPolicyOptions).Substring(0, nameof(HttpTimeoutPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;
    }
}
