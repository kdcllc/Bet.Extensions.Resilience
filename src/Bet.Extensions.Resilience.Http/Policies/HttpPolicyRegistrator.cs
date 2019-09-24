using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpPolicyRegistrator
    {
        private readonly IServiceProvider _provider;

        public HttpPolicyRegistrator(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Register()
        {
            var types = _provider.GetService<HttpPolicyRegistrant>();

            foreach (var type in types.RegisteredPolicies)
            {
                var builder = _provider.GetService(typeof(IHttpPolicyConfigurator<>).MakeGenericType(new Type[] { type.Value }));

                if (builder != null)
                {
                    var method = builder.GetType().GetMethod("ConfigurePolicies");
                    method.Invoke(builder, Array.Empty<object>());
                }
            }
        }
    }
}
