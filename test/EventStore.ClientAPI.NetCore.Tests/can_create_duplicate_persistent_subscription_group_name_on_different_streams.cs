using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class can_create_duplicate_persistent_subscription_group_name_on_different_streams : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();
        protected override void When()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, "group3211", _settings, DefaultData.AdminCredentials).Wait();
            
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.DoesNotThrow(() => _conn.CreatePersistentSubscriptionAsync("someother" + _stream, "group3211", _settings, DefaultData.AdminCredentials).Wait());
        }
    }
}