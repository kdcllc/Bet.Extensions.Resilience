using System;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _output;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper output, string categoryName)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _output.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");
            if (exception != null)
            {
                _output.WriteLine(exception.ToString());
            }
        }

        private class NoopDisposable : IDisposable
        {
#pragma warning disable SA1401 // Fields should be private
            public static NoopDisposable Instance = new NoopDisposable();
#pragma warning restore SA1401 // Fields should be private

            public void Dispose()
            {
            }
        }
    }
}
