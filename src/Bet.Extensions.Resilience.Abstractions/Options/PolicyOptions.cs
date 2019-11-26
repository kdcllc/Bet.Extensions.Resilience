namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// Building block for the <see cref="Polly"/> policies options.
    /// </summary>
    public class PolicyOptions
    {
        /// <summary>
        /// Policy name that associated with the option.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The name of the Options in the Options <see cref="IPolicyOptionsConfigurator{TOptions}{TOptions}"/>.
        /// The 'Name' and 'OptionsName' can be the same.
        /// </summary>
        public string OptionsName { get; set; } = string.Empty;
    }
}
