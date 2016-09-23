using System.Linq;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class when_having_max_count_set_for_stream
    {
        private const string Stream = "max-count-test-stream";
        
        private IEventStoreConnection _connection;
        private EventData[] _testEvents;

        [SetUp]
        public void SetUp()
        {
            _connection = TestConnection.Create(TcpType.Normal);
            _connection.ConnectAsync().Wait();

            _connection.SetStreamMetadataAsync(Stream, ExpectedVersion.Any, StreamMetadata.Build().SetMaxCount(3)).Wait();

            _testEvents = Enumerable.Range(0, 5).Select(x => TestEvent.NewTestEvent(data: x.ToString())).ToArray();
            _connection.AppendToStreamAsync(Stream, ExpectedVersion.Any, _testEvents).Wait();
        }

        [TearDown]
        public  void TearDown()
        {
            _connection.Close();
        }

        [Test]
        public void read_stream_forward_respects_max_count()
        {
            var res = _connection.ReadStreamEventsForwardAsync(Stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(2).Select(x => x.EventId).ToArray(), 
                            res.Events.Select(x => x.Event.EventId).ToArray());
        }

        [Test]
        public void read_stream_backward_respects_max_count()
        {
            var res = _connection.ReadStreamEventsBackwardAsync(Stream, -1, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(2).Select(x => x.EventId).ToArray(),
                            res.Events.Reverse().Select(x => x.Event.EventId).ToArray());
        }

        [Test]
        public void after_setting_less_strict_max_count_read_stream_forward_reads_more_events()
        {
            var res = _connection.ReadStreamEventsForwardAsync(Stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(2).Select(x => x.EventId).ToArray(),
                            res.Events.Select(x => x.Event.EventId).ToArray());

            _connection.SetStreamMetadataAsync(Stream, ExpectedVersion.Any, StreamMetadata.Build().SetMaxCount(4)).Wait();

            res = _connection.ReadStreamEventsForwardAsync(Stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(4, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(1).Select(x => x.EventId).ToArray(),
                            res.Events.Select(x => x.Event.EventId).ToArray());
        }

        [Test]
        public void after_setting_more_strict_max_count_read_stream_forward_reads_less_events()
        {
            var res = _connection.ReadStreamEventsForwardAsync(Stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(2).Select(x => x.EventId).ToArray(),
                            res.Events.Select(x => x.Event.EventId).ToArray());

            _connection.SetStreamMetadataAsync(Stream, ExpectedVersion.Any, StreamMetadata.Build().SetMaxCount(2)).Wait();

            res = _connection.ReadStreamEventsForwardAsync(Stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(2, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(3).Select(x => x.EventId).ToArray(),
                            res.Events.Select(x => x.Event.EventId).ToArray());
        }

        [Test]
        public void after_setting_less_strict_max_count_read_stream_backward_reads_more_events()
        {
            var res = _connection.ReadStreamEventsBackwardAsync(Stream, -1, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(2).Select(x => x.EventId).ToArray(),
                            res.Events.Reverse().Select(x => x.Event.EventId).ToArray());

            _connection.SetStreamMetadataAsync(Stream, ExpectedVersion.Any, StreamMetadata.Build().SetMaxCount(4)).Wait();

            res = _connection.ReadStreamEventsBackwardAsync(Stream, -1, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(4, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(1).Select(x => x.EventId).ToArray(),
                            res.Events.Reverse().Select(x => x.Event.EventId).ToArray());
        }

        [Test]
        public void after_setting_more_strict_max_count_read_stream_backward_reads_less_events()
        {
            var res = _connection.ReadStreamEventsBackwardAsync(Stream, -1, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(2).Select(x => x.EventId).ToArray(),
                            res.Events.Reverse().Select(x => x.Event.EventId).ToArray());

            _connection.SetStreamMetadataAsync(Stream, ExpectedVersion.Any, StreamMetadata.Build().SetMaxCount(2)).Wait();

            res = _connection.ReadStreamEventsBackwardAsync(Stream, -1, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(2, res.Events.Length);
            Assert.AreEqual(_testEvents.Skip(3).Select(x => x.EventId).ToArray(),
                            res.Events.Reverse().Select(x => x.Event.EventId).ToArray());
        }
    }
}
