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
    [TestFixture, Category("LongRunning")]
    public class read_all_events_forward_with_soft_deleted_stream_should : SpecificationWithConnection
    {
        private EventData[] _testEvents;
        private string _stream = "read_all_events_forward_with_soft_deleted_stream_should";
        private Position _position;

        [OneTimeTearDown]
        public void Cleanup()
        {
            _conn.SetStreamMetadataAsync(
                "$all", ExpectedVersion.Any, StreamMetadata.Build(),
                new UserCredentials(SystemUsers.Admin, SystemUsers.DefaultAdminPassword))
            .Wait();
            _conn.Close();
        }

        protected override void When()
        {
            _conn.SetStreamMetadataAsync(
                "$all", ExpectedVersion.Any, StreamMetadata.Build().SetReadRole(SystemRoles.All),
                new UserCredentials(SystemUsers.Admin, SystemUsers.DefaultAdminPassword))
            .Wait();
            _position = _conn.ReadAllEventsBackwardAsync(Position.End, 1, false).Result.NextPosition;
            _testEvents = Enumerable.Range(0, 20).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any, _testEvents).Wait();
            _conn.DeleteStreamAsync(_stream, ExpectedVersion.Any).Wait();
        }

        [Test, Category("LongRunning"), Ignore("TODO: Fix this to work with single ES instance")]
        public void ensure_deleted_stream()
        {
            var res = _conn.ReadStreamEventsForwardAsync(_stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.StreamNotFound, res.Status);
            Assert.AreEqual(0, res.Events.Length);
        }

        [Test, Category("LongRunning")]
        public void returns_all_events_including_tombstone()
        {
            AllEventsSlice read = _conn.ReadAllEventsForwardAsync(_position, _testEvents.Length + 10, false).Result;
            Assert.That(
                EventDataComparer.Equal(
                    _testEvents.ToArray(),
                    read.Events.Skip(read.Events.Length - _testEvents.Length - 1)
                        .Take(_testEvents.Length)
                        .Select(x => x.Event)
                        .ToArray()));
            var lastEvent = read.Events.Last().Event;
            Assert.AreEqual("$$"+ _stream, lastEvent.EventStreamId);
            Assert.AreEqual(SystemEventTypes.StreamMetadata, lastEvent.EventType);
            var metadata = StreamMetadata.FromJsonBytes(lastEvent.Data);
            Assert.AreEqual(EventNumber.DeletedStream, metadata.TruncateBefore);
        }
    }
}
