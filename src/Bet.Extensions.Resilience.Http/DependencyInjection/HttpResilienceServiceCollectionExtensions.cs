using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http;
using Bet.Extensions.Resilience.Http.Policies;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpResilienceServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Http Resilience policy.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="policyName"></param>
        /// <param name="sectionName"></param>
        /// <param name="policyOptionsName"></param>
        /// <param name="configure"></param>
        /// <param name="serviceLifetime"></param>
        public static IServiceCollection AddHttpResiliencePolicy<TType, TImplementation, TOptions>(
            this IServiceCollection services,
            string policyName,
            string sectionName,
            string? policyOptionsName = default,
            Action<TOptions>? configure = default,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TType : IPolicy<TOptions, HttpResponseMessage>
            where TImplementation : BasePolicy<TOptions, HttpResponseMessage>
            where TOptions : PolicyOptions, new()
        {
            return services.AddResiliencePolicy<TType, TImplementation, TOptions, HttpResponseMessage>(
                policyName,
                sectionName,
                policyOptionsName,
                configure,
                serviceLifetime);
        }

        /// <summary>
        /// Add Default Http Policies.
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection AddHttpDefaultResiliencePolicies(
            this IServiceCollection services,
            string policyName = HttpPolicyName.DefaultHttpPolicy,
            string sectionName = HttpPolicyName.DefaultHttpPolicyOptionsName)
        {
            services.AddHttpResiliencePolicy<IHttpTimeoutPolicy<TimeoutPolicyOptions>, HttpTimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                HttpPolicyName.DefaultHttpTimeoutPolicy,
                sectionName,
                HttpPolicyName.DefaultHttpTimeoutPolicy,
                null,
                ServiceLifetime.Transient);

            services.AddHttpResiliencePolicy<IHttpRetryPolicy<RetryPolicyOptions>, HttpRetryPolicy<RetryPolicyOptions>, RetryPolicyOptions>(
                    HttpPolicyName.DefaultHttpRetryPolicy,
                    sectionName,
                    HttpPolicyName.DefaultHttpRetryPolicy,
                    null,
                    ServiceLifetime.Transient);

            services.AddHttpResiliencePolicy<IHttpCircuitBreakerPolicy<CircuitBreakerPolicyOptions>, HttpCircuitBreakerPolicy<CircuitBreakerPolicyOptions>, CircuitBreakerPolicyOptions>(
                    HttpPolicyName.DefaultHttpCircuitBreakerPolicy,
                    sectionName,
                    HttpPolicyName.DefaultHttpCircuitBreakerPolicy,
                    null,
                    ServiceLifetime.Transient);

            return services;
        }

        private static bool IsAdded<T>(this IServiceCollection services)
        {
            return true;
        }
    }
}
