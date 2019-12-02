using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpRetryPolicyOptions : RetryPolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(HttpRetryPolicyOptions).Substring(0, nameof(HttpRetryPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;
    }
}
