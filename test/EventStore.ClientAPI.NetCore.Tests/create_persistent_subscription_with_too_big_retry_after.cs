using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_with_too_big_retry_after : SpecificationWithConnection
    {
        protected override void When()
        {
 
        }
        [Test]
        public void the_build_fails_with_argument_exception()
        {
            Assert.Throws<ArgumentException>(() => PersistentSubscriptionSettings.Create().CheckPointAfter(TimeSpan.FromDays(25 * 365)).Build());
        }
    }
}