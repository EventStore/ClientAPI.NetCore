using System;
using System.Threading;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class deleting_existing_persistent_subscription_with_subscriber : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly ManualResetEvent _called = new ManualResetEvent(false);

        protected override void Given()
        {
            base.Given();
            _conn.CreatePersistentSubscriptionAsync(_stream, "groupname123", _settings,
                DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscription(_stream, "groupname123",
                (s, e) => { },
                (s, r, e) => _called.Set());
        }

        protected override void When()
        {
            _conn.DeletePersistentSubscriptionAsync(_stream, "groupname123", DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_subscription_is_dropped()
        {
            Assert.IsTrue(_called.WaitOne(TimeSpan.FromSeconds(5)));
        }
    }
}