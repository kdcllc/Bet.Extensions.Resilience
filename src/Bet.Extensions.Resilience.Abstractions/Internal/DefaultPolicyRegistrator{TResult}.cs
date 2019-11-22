using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    internal class DefaultPolicyRegistrator<TResult> : IPolicyRegistrator
    {
        private readonly IServiceProvider _provider;

        public DefaultPolicyRegistrator(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void ConfigurePolicies()
        {
            var types = _provider.GetService<PolicyRegistrant>();

            foreach (var type in types.RegisteredPolicies)
            {
                var builder = _provider.GetService(typeof(IPolicyConfigurator<,>).MakeGenericType(new Type[] { type.Value, typeof(TResult), }));

                if (builder != null)
                {
                    var method = builder.GetType().GetMethod("ConfigurePolicies");
                    method.Invoke(builder, Array.Empty<object>());
                }
            }
        }
    }
}
