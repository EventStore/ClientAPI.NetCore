using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_start_from_beginning_and_events_in_it : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromBeginning();

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private ResolvedEvent _firstEvent;
        private List<Guid> _ids = new List<Guid>();
        private bool _set = false;

        private const string _group = "startinbeginning1";

        protected override void Given()
        {
            WriteEvents(_conn);
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();

        }

        private void WriteEvents(IEventStoreConnection connection)
        {
            for (int i = 0; i < 10; i++)
            {
                _ids.Add(Guid.NewGuid());
                connection.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                    new EventData(_ids[i], "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();
            }
        }

        protected override void When()
        {
            _conn.ConnectToPersistentSubscription(
                _stream,
                _group,
                HandleEvent,
                (sub, reason, ex) => { },
                DefaultData.AdminCredentials);
        }

        private Task HandleEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent resolvedEvent)
        {
            if (!_set)
            {
                _set = true;
                _firstEvent = resolvedEvent;
                _resetEvent.Set();
            }
            return Task.CompletedTask;
        }

        [Test]
        public void the_subscription_gets_event_zero_as_its_first_event()
        {
            Assert.IsTrue(_resetEvent.WaitOne(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(0, _firstEvent.Event.EventNumber);
            Assert.AreEqual(_ids[0], _firstEvent.Event.EventId);
        }
    }
}