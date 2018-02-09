using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_non_existing_persistent_subscription_with_permissions_async : SpecificationWithConnection
    {
        private Exception _innerEx;

        protected override void When()
        {
            _innerEx = Assert.Throws<AggregateException>(() =>
            {
                _conn.ConnectToPersistentSubscriptionAsync(
                     "nonexisting2",
                     "foo",
                     (sub, e, i) =>
                     {
                         Console.Write("appeared");
                         return Task.CompletedTask;
                     },
                     (sub, reason, ex) =>
                     {
                     }).Wait();
            }).InnerException;
        }

        [Test]
        public void the_subscription_fails_to_connect_with_argument_exception()
        {
            Assert.IsInstanceOf<AggregateException>(_innerEx);
            Assert.IsInstanceOf<ArgumentException>(_innerEx.InnerException);
        }
    }
}