using System;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// The base class of <see cref="IPolicy{TOptions}"/> interface implementation.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    public abstract class BasePolicy<TOptions> : IPolicy<TOptions>, IDisposable where TOptions : PolicyOptions
    {
        private readonly IPolicyOptionsConfigurator<TOptions> _policyOptionsConfigurator;
        private readonly IPolicyRegistryConfigurator _registryConfigurator;
        private readonly IDisposable _changeTokenRegistration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePolicy{TOptions}"/> class.
        /// </summary>
        /// <param name="policyOptions">The policy options.</param>
        /// <param name="serviceProvider"></param>
        /// <param name="policyOptionsConfigurator">The policy options configurator.</param>
        /// <param name="registryConfigurator">The policy registry configurator.</param>
        /// <param name="logger">The logger.</param>
        public BasePolicy(
            PolicyOptions policyOptions,
            IServiceProvider serviceProvider,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger)
        {
            PolicyOptions = policyOptions ?? throw new ArgumentNullException(nameof(policyOptions));

            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _policyOptionsConfigurator = policyOptionsConfigurator ?? throw new ArgumentNullException(nameof(policyOptionsConfigurator));
            _registryConfigurator = registryConfigurator ?? throw new ArgumentNullException(nameof(registryConfigurator));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // re-register the policies.
            _changeTokenRegistration = ChangeToken.OnChange(
                () => _policyOptionsConfigurator.GetChangeToken(),
                () => ConfigurePolicy());
        }

        /// <inheritdoc/>
        public virtual IPolicyOptionsConfigurator<TOptions> OptionsConfigurator => _policyOptionsConfigurator;

        /// <inheritdoc/>
        public virtual IPolicyRegistryConfigurator PolicyRegistryConfigurator => _registryConfigurator;

        /// <inheritdoc/>
        public virtual string PolicyNameSuffix => "Async";

        /// <inheritdoc/>
        public virtual PolicyOptions PolicyOptions { get; }

        /// <inheritdoc/>
        public virtual TOptions Options => _policyOptionsConfigurator.GetOptions(PolicyOptions.OptionsName);

        public virtual ILogger<IPolicy<TOptions>> Logger { get; }

        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public abstract IAsyncPolicy GetAsyncPolicy();

        /// <inheritdoc/>
        public abstract ISyncPolicy GetSyncPolicy();

        /// <inheritdoc/>
        public virtual void ConfigurePolicy()
        {
            var policyName = $"{PolicyOptions.Name}";

            _registryConfigurator.AddPolicy(policyName, () => GetSyncPolicy(), true);
            Logger.LogDebug("[Configured][Polly Policy] - {policyName}", policyName);

            var asyncPolicy = $"{policyName}{PolicyNameSuffix}";
            _registryConfigurator.AddPolicy(asyncPolicy, () => GetAsyncPolicy(), true);
            Logger.LogDebug("[Configured][Polly Policy] - {policyName}", asyncPolicy);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _changeTokenRegistration?.Dispose();
            }
        }
    }
}
