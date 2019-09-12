using System;
using System.Threading.Tasks;
using Bet.Extensions.Resilience.Abstractions.Policies;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.PolicyBuilders
{
    public class RetryPolicyBuilderTests
    {
        public RetryPolicyBuilderTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        [Fact]
        public void Should_throw_exception_NullReferenceException()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddProvider(new XunitLoggerProvider(Output)));
            var logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("test");
            var pollyContext = new Context();

            pollyContext.AddLogger(logger);

            var policy = RetryPolicyBuilder
                .GetWaitAndRetryForeverAsync<DivideByZeroException>(
                    ex => ex?.Message != null,
                    TimeSpan.FromSeconds(10),
                    $"{nameof(DivideByZeroException)}Policy");

            policy.Invoking(x => throw new NullReferenceException()).Should().Throw<NullReferenceException>();
        }

        [Fact]
        public async Task Should_not_throw_exception()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddProvider(new XunitLoggerProvider(Output)));
            var logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("test");
            var pollyContext = new Context();

            pollyContext.AddLogger(logger);

            var count = 0;

            var policy = RetryPolicyBuilder
                .GetWaitAndRetryForeverAsync<DivideByZeroException>(
                    ex =>
                    {
                        if (count == 1)
                        {
                            return false;
                        }

                        count++;
                        return ex?.Message != null;
                    },
                    TimeSpan.FromSeconds(10),
                    $"{nameof(DivideByZeroException)}Policy");

            // it throws the exception because we get out of the endless loop
            await policy.Invoking(async x => await x.ExecuteAsync((context) => throw new DivideByZeroException(), pollyContext)).Should().ThrowAsync<DivideByZeroException>();
        }
    }
}
