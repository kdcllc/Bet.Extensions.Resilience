﻿using Bet.Extensions.Resilience.Abstractions;

using Microsoft.Extensions.Logging;

namespace Polly
{
    public static class PollyContextExtensions
    {
        /// <summary>
        /// Adds Logger to Polly Context.
        /// </summary>
        /// <param name="context">The Polly execution context.</param>
        /// <param name="logger">The logger to be used by Polly's execution.</param>
        /// <param name="typedClientName">The name of the typed client.</param>
        /// <returns></returns>
        public static Context AddTypedHttpClientLogger(
            this Context context,
            ILogger logger,
            string typedClientName = default)
        {
            if (context == null)
            {
                return null;
            }

            context[PolicyContextItems.Logger] = logger;
            context[PolicyContextItems.HttpClientName] = typedClientName ?? "Untracked";
            return context;
        }

        /// <summary>
        /// Attempts to get registered Logger from Polly's context.
        /// </summary>
        /// <param name="context">The Polly execution context.</param>
        /// <param name="logger">The logger to be used by Polly's execution.</param>
        /// <returns>bool</returns>
        public static bool TryGetLogger(this Context context, out ILogger logger)
        {
            if (context.TryGetValue(PolicyContextItems.Logger, out var loggerObject)
                && loggerObject is ILogger theLogger)
            {
                logger = theLogger;
                return true;
            }

            logger = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the name of the Http Typed Client from Polly's context.
        /// </summary>
        /// <param name="context">The Polly execution context.</param>
        /// <param name="typedHttpClientName">The name of the typed client.</param>
        /// <returns>bool</returns>
        public static bool TryGetTypedHttpClientName(this Context context, out string typedHttpClientName)
        {
            if (context.TryGetValue(PolicyContextItems.HttpClientName, out var name)
                && name is string theName)
            {
                typedHttpClientName = theName;
                return true;
            }

            typedHttpClientName = "Untracked";
            return false;
        }
    }
}