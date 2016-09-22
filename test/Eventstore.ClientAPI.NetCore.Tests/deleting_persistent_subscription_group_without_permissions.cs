using System;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class deleting_persistent_subscription_group_without_permissions : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();

        protected override void When()
        {
        }

        [Test]
        public void the_delete_fails_with_access_denied()
        {
            try
            {
                _conn.DeletePersistentSubscriptionAsync(_stream, Guid.NewGuid().ToString()).Wait();
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