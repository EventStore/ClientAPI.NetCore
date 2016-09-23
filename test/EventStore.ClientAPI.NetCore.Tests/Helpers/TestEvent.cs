using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;

namespace Eventstore.ClientAPI.Tests.Helpers
{
    public class TestEvent
    {
        public static EventData NewTestEvent(string data = null, string metadata = null)
        {
            return NewTestEvent(Guid.NewGuid(), data, metadata);
        }

        public static EventData NewTestEvent(Guid eventId, string data = null, string metadata = null)
        {
            var encodedData = Helper.UTF8NoBom.GetBytes(data ?? eventId.ToString());
            var encodedMetadata = Helper.UTF8NoBom.GetBytes(metadata ?? "metadata");

            return new EventData(eventId, "TestEvent", false, encodedData, encodedMetadata);
        }
    }
}
