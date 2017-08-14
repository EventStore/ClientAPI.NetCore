using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_group_without_permissions : SpecificationWithConnection
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
            try
            {
                _conn.CreatePersistentSubscriptionAsync(_stream, "group57", _settings, null).Wait();
                throw new Exception("expected exception");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf(typeof(AggregateException), ex);
                var inner = ex.InnerException;
                Assert.IsInstanceOf(typeof(AccessDeniedException), inner);
            }
        }
    }
}