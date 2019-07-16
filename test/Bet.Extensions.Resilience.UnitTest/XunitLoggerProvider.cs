using System;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public TestLoggerProvider(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_output, categoryName);
        }

        public void Dispose()
        { }
    }
}
