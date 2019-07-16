using System;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public XunitLoggerProvider(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output, categoryName);
        }

        public void Dispose()
        { }
    }
}
