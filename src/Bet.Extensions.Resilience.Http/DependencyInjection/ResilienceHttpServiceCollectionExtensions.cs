using Bet.Extensions.Http.MessageHandlers.HttpTimeout;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Options;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceHttpServiceCollectionExtensions
    {
        /// <summary>
        /// Add Default Http Policies.
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddHttpDefaultResiliencePolicies(
            this IServiceCollection services,
            string sectionName = HttpPolicyOptionsKeys.DefaultHttpPolicy)
        {
            services.AddPollyPolicy<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>(HttpPolicyOptionsKeys.HttpTimeoutPolicy)
                        .ConfigurePolicy(
                            sectionName: $"{sectionName}:{HttpPolicyOptionsKeys.HttpTimeoutPolicy}",
                            (policy) => PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

            services.AddPollyPolicy<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions>(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy)
                        .ConfigurePolicy(
                            sectionName: $"{sectionName}:{HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy}",
                            (policy) => policy.HttpCreateCircuitBreakerAsync());

            services.AddPollyPolicy<AsyncRetryPolicy<HttpResponseMessage>, RetryPolicyOptions>(HttpPolicyOptionsKeys.HttpRetryPolicy)
                        .ConfigurePolicy(
                            sectionName: $"{sectionName}:{HttpPolicyOptionsKeys.HttpRetryPolicy}",
                            (policy) => policy.HttpCreateRetryAsync());

            return services;
        }

        public static IHttpClientBuilder AddHttpTimeoutHanlder<TPolicyOptions>(this IHttpClientBuilder builder) where TPolicyOptions : TimeoutPolicyOptions, new()
        {
            // register delegating handler options.
            builder.Services.AddSingleton<IConfigureOptions<HttpTimeoutHandlerOptions>>((sp) =>
            {
                var timeoutPolicyOptions = sp.GetRequiredService<IOptions<TPolicyOptions>>().Value;
                return new ConfigureOptions<HttpTimeoutHandlerOptions>((options) =>
                {
                    options.DefaultTimeout = timeoutPolicyOptions.Timeout;
                });
            });

            builder.AddHttpTimeoutHandler();

            return builder;
        }
    }
}
