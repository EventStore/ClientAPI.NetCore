using System;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class a_nak_in_subscription_handler_in_autoack_mode_drops_the_subscription : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromBeginning();

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private Exception _exception;
        private SubscriptionDropReason _reason;

        private const string _group = "naktest";

        protected override void Given()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscription(
                _stream,
                _group,
                HandleEvent,
                Dropped,
                DefaultData.AdminCredentials);

        }

        private void Dropped(EventStorePersistentSubscriptionBase sub, SubscriptionDropReason reason, Exception exception)
        {
            _exception = exception;
            _reason = reason;
            _resetEvent.Set();
        }

        protected override void When()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                new EventData(Guid.NewGuid(), "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();

        }

        private static void HandleEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent resolvedEvent)
        {
            throw new Exception("test");
        }

        [Test]
        public void the_subscription_gets_dropped()
        {
            Assert.IsTrue(_resetEvent.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(SubscriptionDropReason.EventHandlerException, _reason);
            Assert.AreEqual(typeof(Exception), _exception.GetType());
            Assert.AreEqual("test", _exception.Message);
        }

    }
}