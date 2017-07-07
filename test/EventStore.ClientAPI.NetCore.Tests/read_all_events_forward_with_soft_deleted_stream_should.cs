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

            _testEvents = Enumerable.Range(0, 20).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any, _testEvents).Wait();
            _conn.DeleteStreamAsync(_stream, ExpectedVersion.Any).Wait();
        }

        [Test, Category("LongRunning")]
        public void ensure_deleted_stream()
        {
            var res = _conn.ReadStreamEventsForwardAsync(_stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.StreamNotFound, res.Status);
            Assert.AreEqual(0, res.Events.Length);
        }

        [Test, Category("LongRunning")]
        public void returns_all_events_including_tombstone()
        {
            var metadataEvents = _conn.ReadStreamEventsBackwardAsync("$$" + _stream, -1, 1, true, new UserCredentials("admin", "changeit")).Result;
            var lastEvent = metadataEvents.Events[0].Event;
            Assert.AreEqual("$$"+ _stream, lastEvent.EventStreamId);
            Assert.AreEqual(SystemEventTypes.StreamMetadata, lastEvent.EventType);
            var metadata = StreamMetadata.FromJsonBytes(lastEvent.Data);
            Assert.AreEqual(EventNumber.DeletedStream, metadata.TruncateBefore);
        }
    }
}
