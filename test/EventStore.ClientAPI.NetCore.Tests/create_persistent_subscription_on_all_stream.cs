using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_on_all_stream : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();
        protected override void When()
        {

        }

        [Test]
        public void the_completion_fails_with_invalid_stream()
        {
            Assert.Throws<AggregateException>(() => _conn.CreatePersistentSubscriptionAsync("$all", "shitbird", _settings, DefaultData.AdminCredentials).Wait());
        }
    }
}