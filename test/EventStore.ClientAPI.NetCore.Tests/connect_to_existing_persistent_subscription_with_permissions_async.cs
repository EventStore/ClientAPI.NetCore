using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_permissions_async : SpecificationWithConnection
    {
        private EventStorePersistentSubscriptionBase _sub;
        private readonly string _stream = Guid.NewGuid().ToString();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        protected override void When()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, "agroupname17", _settings, DefaultData.AdminCredentials).Wait();
            _sub = _conn.ConnectToPersistentSubscriptionAsync(_stream,
                "agroupname17",
                (sub, e) => Console.Write("appeared"),
                (sub, reason, ex) => { }).Result;
        }

        [Test]
        public void the_subscription_suceeds()
        {
            Assert.IsNotNull(_sub);
        }
    }
}