using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_on_non_existing_stream : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();
        protected override void When()
        {
            
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.DoesNotThrow(() => _conn.CreatePersistentSubscriptionAsync(_stream, "nonexistinggroup", _settings, DefaultData.AdminCredentials).Wait());
        }
    }
}