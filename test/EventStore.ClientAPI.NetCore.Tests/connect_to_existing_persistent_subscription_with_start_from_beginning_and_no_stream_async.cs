using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_start_from_beginning_and_no_stream_async : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromBeginning();

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private ResolvedEvent _firstEvent;
        private readonly Guid _id = Guid.NewGuid();
        private bool _set = false;

        private const string _group = "startinbeginning4";

        protected override void Given()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscriptionAsync(
                _stream,
                _group,
                HandleEvent,
                (sub, reason, ex) => { },
                DefaultData.AdminCredentials).Wait();
        }

        protected override void When()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                new EventData(_id, "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();
        }

        private Task HandleEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent resolvedEvent)
        {
            if (_set)
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
            Assert.AreEqual(_id, _firstEvent.Event.EventId);
        }
    }
}