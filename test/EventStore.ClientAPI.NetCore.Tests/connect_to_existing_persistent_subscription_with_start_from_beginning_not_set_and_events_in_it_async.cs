using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_start_from_beginning_not_set_and_events_in_it_async : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

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
                connection.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                    new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();
            }
        }

        protected override void When()
        {
            _conn.ConnectToPersistentSubscriptionAsync(
                _stream,
                _group,
                HandleEvent,
                (sub, reason, ex) => { },
                DefaultData.AdminCredentials).Wait();
        }

        private Task HandleEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent resolvedEvent)
        {
            _resetEvent.Set();
            return Task.CompletedTask;
        }

        [Test]
        public void the_subscription_gets_no_events()
        {
            Assert.IsFalse(_resetEvent.WaitOne(TimeSpan.FromSeconds(1)));
        }
    }
}