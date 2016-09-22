using System.Linq;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common;
using EventStore.ClientAPI.SystemData;
using NUnit.Framework;
using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;
using StreamMetadata = EventStore.ClientAPI.StreamMetadata;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning"), Ignore("Uses $all")]
    public class read_all_events_forward_with_hard_deleted_stream_should : SpecificationWithConnection
    {
        private EventData[] _testEvents;
        private string _streamName = "read_all_events_forward_with_hard_deleted_stream_should";

        protected override void When()
        {
            _conn.SetStreamMetadataAsync(
                "$all", -1, StreamMetadata.Build().SetReadRole(SystemRoles.All),
                new UserCredentials(SystemUsers.Admin, SystemUsers.DefaultAdminPassword))
            .Wait();

            _testEvents = Enumerable.Range(0, 20).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
            _conn.AppendToStreamAsync(_streamName, ExpectedVersion.EmptyStream, _testEvents).Wait();
            _conn.DeleteStreamAsync(_streamName, ExpectedVersion.Any, hardDelete: true).Wait();
        }

        [Test, Category("LongRunning")]
        public void ensure_deleted_stream()
        {
            var res = _conn.ReadStreamEventsForwardAsync(_streamName, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.StreamDeleted, res.Status);
            Assert.AreEqual(0, res.Events.Length);
        }

        [Test, Category("LongRunning")]
        public void returns_all_events_including_tombstone()
        {
            AllEventsSlice read = _conn.ReadAllEventsForwardAsync(Position.Start, _testEvents.Length + 10, false).Result;
            Assert.That(
                EventDataComparer.Equal(
                    _testEvents.ToArray(),
                    read.Events.Skip(read.Events.Length - _testEvents.Length - 1)
                        .Take(_testEvents.Length)
                        .Select(x => x.Event)
                        .ToArray()));
            var lastEvent = read.Events.Last().Event;
            Assert.AreEqual(_streamName, lastEvent.EventStreamId);
            Assert.AreEqual(SystemEventTypes.StreamDeleted, lastEvent.EventType);
        }
    }
}
