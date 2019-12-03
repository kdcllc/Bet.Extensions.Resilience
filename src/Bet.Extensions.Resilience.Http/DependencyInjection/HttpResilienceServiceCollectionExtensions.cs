using System;
using System.Net.Http;
using Bet.Extensions.Http.MessageHandlers.HttpTimeout;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.Http.Policies;
using Microsoft.Extensions.Options;

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
            string policyName = HttpPolicyNames.DefaultHttpPolicy,
            string sectionName = HttpPolicyNames.DefaultHttpPolicyOptionsName)
        {
            services.AddHttpResiliencePolicy<IHttpTimeoutPolicy, HttpTimeoutPolicy, HttpTimeoutPolicyOptions>(
                HttpTimeoutPolicyOptions.DefaultName,
                sectionName,
                HttpTimeoutPolicyOptions.DefaultName,
                null,
                ServiceLifetime.Transient);

            services.AddHttpResiliencePolicy<IHttpRetryPolicy, HttpRetryPolicy, HttpRetryPolicyOptions>(
                    HttpRetryPolicyOptions.DefaultName,
                    sectionName,
                    HttpRetryPolicyOptions.DefaultName,
                    null,
                    ServiceLifetime.Transient);

            services.AddHttpResiliencePolicy<IHttpCircuitBreakerPolicy, HttpCircuitBreakerPolicy, HttpCircuitBreakerPolicyOptions>(
                    HttpCircuitBreakerPolicyOptions.DefaultName,
                    sectionName,
                    HttpCircuitBreakerPolicyOptions.DefaultName,
                    null,
                    ServiceLifetime.Transient);

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
