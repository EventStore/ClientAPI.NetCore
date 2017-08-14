using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class update_non_existing_persistent_subscription : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        protected override void When()
        {
            
        }

        [Test]
        public void the_completion_fails_with_not_found()
        {
            try
            {
                _conn.UpdatePersistentSubscriptionAsync(_stream, "existing", _settings,
                    DefaultData.AdminCredentials).Wait();
                Assert.Fail("should have thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<AggregateException>(ex);
                Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            }
        }
    }
}