using System;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class update_existing_persistent_subscription_without_permissions : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        protected override void When()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "whatever", true, Encoding.UTF8.GetBytes("{'foo' : 2}"), new Byte[0]));
            _conn.CreatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_completion_fails_with_access_denied()
        {
            try
            {
                _conn.UpdatePersistentSubscriptionAsync(_stream, "existing", _settings, null).Wait();
                Assert.Fail("should have thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<AggregateException>(ex);
                Assert.IsInstanceOf<AccessDeniedException>(ex.InnerException);
            }
        }
    }
}