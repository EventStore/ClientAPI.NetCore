using System;
using System.Text;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class update_existing_persistent_subscription : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromCurrent();

        protected override void Given()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "whatever", true, Encoding.UTF8.GetBytes("{'foo' : 2}"), new Byte[0]));
            _conn.CreatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials).Wait();
        }

        protected override void When()
        {
            
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.DoesNotThrow(() => _conn.UpdatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials).Wait());
        }
    }
}