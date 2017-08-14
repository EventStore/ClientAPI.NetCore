using System;
using System.Linq;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.Core.Tests.ClientAPI.Helpers;
using NUnit.Framework;
using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;
using StreamMetadata = EventStore.ClientAPI.StreamMetadata;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class read_stream_events_with_unresolved_linkto : SpecificationWithConnection
    {
        private EventData[] _testEvents;
        private string _stream;
        private string _links;

        protected override void When()
        {
            _conn.SetStreamMetadataAsync(
                "$all", ExpectedVersion.Any, StreamMetadata.Build().SetReadRole(SystemRoles.All),
                new UserCredentials(SystemUsers.Admin, SystemUsers.DefaultAdminPassword))
            .Wait();

            _testEvents = Enumerable.Range(0, 20).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
            _stream = "read_stream_events_with_unresolved_linkto";
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.EmptyStream, _testEvents).Wait();
            _links = "read_stream_events_with_unresolved_linkto_links";
            _conn.AppendToStreamAsync(
                _links, ExpectedVersion.EmptyStream,
                new EventData(
                    Guid.NewGuid(), EventStore.ClientAPI.Common.SystemEventTypes.LinkTo, false,
                    Encoding.UTF8.GetBytes("0@read_stream_events_with_unresolved_linkto"), null))
            .Wait();
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
        public void returns_unresolved_linkto()
        {
            var read = _conn.ReadStreamEventsForwardAsync(_links, 0, 1, true).Result;
            Assert.AreEqual(1, read.Events.Length);
            Assert.IsNull(read.Events[0].Event);
            Assert.IsNotNull(read.Events[0].Link);
        }
    }
}
