using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Hosting
{
    internal sealed class HostingEventSource : EventSource
    {
        public static readonly HostingEventSource Log = new HostingEventSource();

        private long _totalRequests;
        private long _currentRequests;
        private long _failedRequests;

        internal HostingEventSource()
            : this("Microsoft.Extensions.Hosting")
        {
        }

        // Used for testing
        internal HostingEventSource(string eventSourceName)
            : base(eventSourceName)
        {
        }

        // NOTE
        // - The 'Start' and 'Stop' suffixes on the following event names have special meaning in EventSource. They
        //   enable creating 'activities'.
        //   For more information, take a look at the following blog post:
        //   https://blogs.msdn.microsoft.com/vancem/2015/09/14/exploring-eventsource-activity-correlation-and-causation-features/
        // - A stop event's event id must be next one after its start event.
        [Event(1, Level = EventLevel.Informational)]
        public void HostStart()
        {
            WriteEvent(1);
        }

        [Event(2, Level = EventLevel.Informational)]
        public void HostStop()
        {
            WriteEvent(2);
        }

        [Event(3, Level = EventLevel.Informational)]
        public void RequestStart(string method, string path)
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _currentRequests);
            WriteEvent(3, method, path);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(4, Level = EventLevel.Informational)]
        public void RequestStop()
        {
            Interlocked.Decrement(ref _currentRequests);
            WriteEvent(4);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Event(5, Level = EventLevel.Error)]
        public void UnhandledException()
        {
            WriteEvent(5);
        }

        internal void RequestFailed()
        {
            Interlocked.Increment(ref _failedRequests);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
            }
        }
    }
}
