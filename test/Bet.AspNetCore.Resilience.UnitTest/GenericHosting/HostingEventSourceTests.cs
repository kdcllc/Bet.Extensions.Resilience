using System.Diagnostics.Tracing;

using Microsoft.Extensions.Hosting;

using Xunit;

namespace Bet.AspNetCore.Resilience.UnitTest.GenericHosting
{
    public class HostingEventSourceTests
    {
        [Fact]
        public void MatchesNameAndGuid()
        {
            // Arrange & Act
            var eventSource = new HostingEventSource();

            // Assert
            Assert.Equal("Microsoft.Extensions.Hosting", eventSource.Name);
            Assert.Equal(Guid.Parse("d6979dec-fb63-5994-1634-887646f0ac5e"), eventSource.Guid);
        }

        [Fact]
        public void HostStart()
        {
            // Arrange
            var expectedEventId = 1;
            var eventListener = new TestEventListener(expectedEventId);
            var hostingEventSource = GetHostingEventSource();
            eventListener.EnableEvents(hostingEventSource, EventLevel.Informational);

            // Act
            hostingEventSource.HostStart();

            // Assert
            var eventData = eventListener.EventData;
            Assert.NotNull(eventData);
            Assert.Equal(expectedEventId, eventData.EventId);
            Assert.Equal("HostStart", eventData.EventName);
            Assert.Equal(EventLevel.Informational, eventData.Level);
            Assert.Same(hostingEventSource, eventData.EventSource);
            Assert.Null(eventData.Message);
            Assert.Empty(eventData.Payload);
        }

        [Fact]
        public void HostStop()
        {
            // Arrange
            var expectedEventId = 2;
            var eventListener = new TestEventListener(expectedEventId);
            var hostingEventSource = GetHostingEventSource();
            eventListener.EnableEvents(hostingEventSource, EventLevel.Informational);

            // Act
            hostingEventSource.HostStop();

            // Assert
            var eventData = eventListener.EventData;
            Assert.NotNull(eventData);
            Assert.Equal(expectedEventId, eventData.EventId);
            Assert.Equal("HostStop", eventData.EventName);
            Assert.Equal(EventLevel.Informational, eventData.Level);
            Assert.Same(hostingEventSource, eventData.EventSource);
            Assert.Null(eventData.Message);
            Assert.Empty(eventData.Payload);
        }

        private static HostingEventSource GetHostingEventSource()
        {
            return new HostingEventSource(Guid.NewGuid().ToString());
        }
    }
}
