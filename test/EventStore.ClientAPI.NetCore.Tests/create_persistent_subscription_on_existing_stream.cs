using System;
using System.Text;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_on_existing_stream : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        protected override void When()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "whatever", true, Encoding.UTF8.GetBytes("{'foo' : 2}"), new Byte[0]));
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.DoesNotThrow(
                () =>
                    _conn.CreatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials)
                        .Wait());
        }
    }
}
