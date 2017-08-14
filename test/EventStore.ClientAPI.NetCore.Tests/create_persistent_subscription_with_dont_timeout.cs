using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_with_dont_timeout : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent()
            .DontTimeoutMessages();
        protected override void When()
        {
        } 

        [Test]
        public void the_message_timeout_should_be_zero()
        {
            Assert.That(_settings.MessageTimeout == TimeSpan.Zero);
        }

        [Test]
        public void the_subscription_is_created_without_error()
        {
            Assert.DoesNotThrow(
                () => 
                    _conn.CreatePersistentSubscriptionAsync(_stream, "dont-timeout", _settings, DefaultData.AdminCredentials).Wait()
            );
        }
    }
}