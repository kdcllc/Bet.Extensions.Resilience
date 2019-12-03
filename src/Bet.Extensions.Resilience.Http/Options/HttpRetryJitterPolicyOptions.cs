using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpRetryJitterPolicyOptions : RetryJitterPolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(HttpRetryJitterPolicyOptions).Substring(0, nameof(HttpRetryJitterPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;
    }
}
