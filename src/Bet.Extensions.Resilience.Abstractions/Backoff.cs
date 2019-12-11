﻿using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Abstractions
{
    public static class Backoff
    {
        /// <summary>
        /// Creates Jittered timespan.
        /// https://github.com/App-vNext/Polly/wiki/Retry-with-jitter/86fc200c672e87cc24fe444ec61f38da19e861ec#more-complex-jitter.
        /// </summary>
        /// <param name="maxRetries"></param>
        /// <param name="seedDelay"></param>
        /// <param name="maxDelay"></param>
        /// <returns></returns>
        public static IEnumerable<TimeSpan> DecorrelatedJitter(int maxRetries, TimeSpan seedDelay, TimeSpan maxDelay)
        {
            var jitterer = new Random();
            var retries = 0;

            var seed = seedDelay.TotalMilliseconds;
            var max = maxDelay.TotalMilliseconds;
            var current = seed;

            while (++retries <= maxRetries)
            {
                // adopting the 'Decorrelated Jitter' formula from https://www.awsarchitectureblog.com/2015/03/backoff.html.  Can be between seed and previous * 3.  Mustn't exceed max.
                current = Math.Min(max, Math.Max(seed, current * 3 * jitterer.NextDouble()));
                yield return TimeSpan.FromMilliseconds(current);
            }
        }

        /// <summary>
        /// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
        /// For example: 850ms, 1455ms, 3060ms.
        /// Per discussion in Polly issue 530, the jitter of this implementation exhibits fewer spikes and a smoother distribution than the AWS jitter formula.
        /// </summary>
        /// <param name="medianFirstRetryDelay">The median delay to target before the first retry, call it f (= f * 2^0).
        /// Choose this value both to approximate the first delay, and to scale the remainder of the series.
        /// Subsequent retries will (over a large sample size) have a median approximating retries at time f * 2^1, f * 2^2 ... f * 2^t etc for try t.
        /// The actual amount of delay-before-retry for try t may be distributed between 0 and f * (2^(t+1) - 2^(t-1)) for t >= 2;
        /// or between 0 and f * 2^(t+1), for t is 0 or 1.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> DecorrelatedJitterBackoffV2(
            TimeSpan medianFirstRetryDelay,
            int retryCount,
            int? seed = null,
            bool fastFirst = false)
        {
            if (medianFirstRetryDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(medianFirstRetryDelay), medianFirstRetryDelay, "should be >= 0ms");
            }

            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");
            }

            if (retryCount == 0)
            {
                return Empty();
            }

            return Enumerate(medianFirstRetryDelay, retryCount, fastFirst, new ConcurrentRandom(seed));

            // The original author/credit for this jitter formula is @george-polevoy . Jitter formula used with permission as described at https://github.com/App-vNext/Polly/issues/530#issuecomment-526555979
            // Minor adaptations (pFactor = 4.0 and rpScalingFactor = 1 / 1.4d) by @reisenberger, to scale the formula output for easier parameterisation to users.
            IEnumerable<TimeSpan> Enumerate(TimeSpan scaleFirstTry, int maxRetries, bool fast, ConcurrentRandom random)
            {
                // A factor used within the formula to help smooth the first calculated delay.
                const double pFactor = 4.0;

                // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid Polly user comprehension.
                // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
                const double rpScalingFactor = 1 / 1.4d;

                var i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                var targetTicksFirstDelay = scaleFirstTry.Ticks;

                var prev = 0.0;
                for (; i < maxRetries; i++)
                {
                    var t = (double)i + random.NextDouble();
                    var next = Math.Pow(2, t) * Math.Tanh(Math.Sqrt(pFactor * t));

                    var formulaIntrinsicValue = next - prev;
                    yield return TimeSpan.FromTicks((long)(formulaIntrinsicValue * rpScalingFactor * targetTicksFirstDelay));

                    prev = next;
                }
            }
        }

        private static IEnumerable<TimeSpan> Empty()
        {
            yield break;
        }
    }
}
