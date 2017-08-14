using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_start_from_x_set_higher_than_x_and_events_in_it_then_event_written : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFrom(11);

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private ResolvedEvent _firstEvent;
        private Guid _id;

        private const string _group = "startinbeginning1";

        protected override void Given()
        {
            WriteEvents(_conn);
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscription(
                _stream,
                _group,
                HandleEvent,
                (sub, reason, ex) => { },
                DefaultData.AdminCredentials);

        }

        private void WriteEvents(IEventStoreConnection connection)
        {
            for (int i = 0; i < 11; i++)
            {
                connection.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                    new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();
            }
        }

        protected override void When()
        {
            _id = Guid.NewGuid();
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                new EventData(_id, "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();

        }

        private Task HandleEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent resolvedEvent)
        {
            _firstEvent = resolvedEvent;
            _resetEvent.Set();
            return Task.CompletedTask;
        }

        [Test]
        public void the_subscription_gets_the_written_event_as_its_first_event()
        {
            Assert.IsTrue(_resetEvent.WaitOne(TimeSpan.FromSeconds(10)));
            Assert.IsNotNull(_firstEvent);
            Assert.AreEqual(11, _firstEvent.Event.EventNumber);
            Assert.AreEqual(_id, _firstEvent.Event.EventId);
        }
    }
}