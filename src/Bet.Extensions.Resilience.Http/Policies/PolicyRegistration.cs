using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class PolicyRegistration
    {
        private readonly IServiceProvider _provider;

        public PolicyRegistration(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Register()
        {
            var types = _provider.GetService<ResilienceHttpPolicyRegistrant>();

            foreach (var type in types.RegisteredPolicies)
            {
                var builder = _provider.GetService(typeof(IResilienceHttpPolicyBuilder<>).MakeGenericType(new Type[] { type }));

                if (builder != null)
                {
                    var method = builder.GetType().GetMethod("RegisterPolicies");
                    method.Invoke(builder, Array.Empty<object>());
                }
            }
        }
    }
}
