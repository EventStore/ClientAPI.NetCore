using System;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class when_committing_empty_transaction
    {
        private IEventStoreConnection _connection;
        private EventData _firstEvent;
        private string _stream;

        [SetUp]
        public void SetUp()
        {
            _firstEvent = TestEvent.NewTestEvent();

            _connection = BuildConnection();
            _connection.ConnectAsync().Wait();
            _stream = TestContext.CurrentContext.Test.FullName;
            Assert.AreEqual(2, _connection.AppendToStreamAsync(_stream,
                                                          ExpectedVersion.NoStream,
                                                          _firstEvent,
                                                          TestEvent.NewTestEvent(),
                                                          TestEvent.NewTestEvent()).Result.NextExpectedVersion);

            using (var transaction = _connection.StartTransactionAsync(_stream, 2).Result)
            {
                Assert.AreEqual(2, transaction.CommitAsync().Result.NextExpectedVersion);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
        }

        protected virtual IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [Test]
        public void following_append_with_correct_expected_version_are_commited_correctly()
        {
            Assert.AreEqual(4, _connection.AppendToStreamAsync(_stream, 2, TestEvent.NewTestEvent(), TestEvent.NewTestEvent()).Result.NextExpectedVersion);

            var res = _connection.ReadStreamEventsForwardAsync(_stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(5, res.Events.Length);
            for (int i=0; i<5; ++i)
            {
                Assert.AreEqual(i, res.Events[i].OriginalEventNumber);
            }
        }

        [Test]
        public void following_append_with_expected_version_any_are_commited_correctly()
        {
            Assert.AreEqual(4, _connection.AppendToStreamAsync(_stream, ExpectedVersion.Any, TestEvent.NewTestEvent(), TestEvent.NewTestEvent()).Result.NextExpectedVersion);

            var res = _connection.ReadStreamEventsForwardAsync(_stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(5, res.Events.Length);
            for (int i = 0; i < 5; ++i)
            {
                Assert.AreEqual(i, res.Events[i].OriginalEventNumber);
            }
        }

        [Test]
        public void committing_first_event_with_expected_version_no_stream_is_idempotent()
        {
            Assert.AreEqual(0, _connection.AppendToStreamAsync(_stream, ExpectedVersion.NoStream, _firstEvent).Result.NextExpectedVersion);

            var res = _connection.ReadStreamEventsForwardAsync(_stream, 0, 100, false).Result;
            Assert.AreEqual(SliceReadStatus.Success, res.Status);
            Assert.AreEqual(3, res.Events.Length);
            for (int i = 0; i < 3; ++i)
            {
                Assert.AreEqual(i, res.Events[i].OriginalEventNumber);
            }
        }

        [Test]
        public void trying_to_append_new_events_with_expected_version_no_stream_fails()
        {
            Assert.That(() => _connection.AppendToStreamAsync(_stream, ExpectedVersion.NoStream, TestEvent.NewTestEvent()).Result,
                        Throws.Exception.InstanceOf<AggregateException>()
                        .With.InnerException.InstanceOf<WrongExpectedVersionException>());
        }
    }
}
