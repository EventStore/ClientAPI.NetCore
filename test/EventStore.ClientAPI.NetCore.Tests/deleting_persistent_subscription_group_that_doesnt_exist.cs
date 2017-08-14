using System;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class deleting_persistent_subscription_group_that_doesnt_exist : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();

        protected override void When()
        {
        }

        [Test]
        public void the_delete_fails_with_argument_exception()
        {
            try
            {
                _conn.DeletePersistentSubscriptionAsync(_stream, Guid.NewGuid().ToString(), DefaultData.AdminCredentials).Wait();
                throw new Exception("expected exception");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf(typeof(AggregateException), ex);
                var inner = ex.InnerException;
                Assert.IsInstanceOf(typeof(InvalidOperationException), inner);
            }
        }
    }
}