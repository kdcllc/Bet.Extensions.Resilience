namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// Bulk Head Policy Options.
    /// </summary>
    public class BulkheadPolicyOptions
    {
        public int MaxParallelization { get; set; } = 100;

        public int MaxQueuedItems { get; set; } = 100;
    }
}
