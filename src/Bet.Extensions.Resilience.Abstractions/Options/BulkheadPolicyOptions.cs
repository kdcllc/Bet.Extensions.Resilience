namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// The <see cref="Polly.Bulkhead.BulkheadPolicy"/> Policy Options.
    /// Premise: 'One fault shouldn't bring down the whole ship!'.
    /// https://github.com/App-vNext/Polly/wiki/Bulkhead.
    /// </summary>
    public class BulkheadPolicyOptions : PolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(BulkheadPolicyOptions).Substring(0, nameof(BulkheadPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;

        /// <summary>
        /// The maximum parallelization of executions through the bulkhead.
        /// </summary>
        public int MaxParallelization { get; set; } = 100;

        /// <summary>
        /// The maximum number of actions that may be queuing (waiting to acquire an execution slot) at any time. (optional).
        /// </summary>
        public int MaxQueuedItems { get; set; } = 100;
    }
}
