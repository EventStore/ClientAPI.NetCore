using System;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class update_existing_persistent_subscription_with_subscribers : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();
        private readonly AutoResetEvent _dropped = new AutoResetEvent(false);
        private SubscriptionDropReason _reason;
        private Exception _exception;
        private Exception _caught = null;

        protected override void Given()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "whatever", true, Encoding.UTF8.GetBytes("{'foo' : 2}"), new Byte[0]));
            _conn.CreatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscription(_stream, "existing" , (x, y) => { },
                (sub, reason, ex) =>
                {
                    _dropped.Set();
                    _reason = reason;
                    _exception = ex;
                });
        }

        protected override void When()
        {
            try
            {
                _conn.UpdatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials)
                    .Wait();
            }
            catch (Exception ex)
            {
                _caught = ex;
            }
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.IsNull(_caught);
        }

        [Test]
        public void existing_subscriptions_are_dropped()
        {
            Assert.IsTrue(_dropped.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(SubscriptionDropReason.UserInitiated, _reason);
            Assert.IsNull(_exception);
        }

    }
}