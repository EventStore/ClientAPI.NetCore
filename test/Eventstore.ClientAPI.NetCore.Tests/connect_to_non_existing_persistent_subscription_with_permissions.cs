using System;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_non_existing_persistent_subscription_with_permissions : SpecificationWithConnection
    {
        private Exception _caught;

        protected override void When()
        {
            _caught = Assert.Throws<AggregateException>(() =>
            {
                _conn.ConnectToPersistentSubscription(
                    "nonexisting2",
                    "foo",
                    (sub, e) => Console.Write("appeared"),
                    (sub, reason, ex) =>
                    {
                    });
                throw new Exception("should have thrown");
            }).InnerException;
        }

        [Test]
        public void the_completion_fails()
        {
            Assert.IsNotNull(_caught);
        }

        [Test]
        public void the_exception_is_an_argument_exception()
        {
            Assert.IsInstanceOf<ArgumentException>(_caught.InnerException);
        }
    }


    //ALL

/*

    [TestFixture, Category("LongRunning")]
    public class connect_to_non_existing_persistent_all_subscription_with_permissions : SpecificationWithConnection
    {
        private Exception _caught;

        protected override void When()
        {
            try
            {
                _conn.ConnectToPersistentSubscriptionForAll("nonexisting2",
                    (sub, e) => Console.Write("appeared"),
                    (sub, reason, ex) =>
                    {
                    }, 
                    DefaultData.AdminCredentials);
                throw new Exception("should have thrown");
            }
            catch (Exception ex)
            {
                _caught = ex;
            }
        }

        [Test]
        public void the_completion_fails()
        {
            Assert.IsNotNull(_caught);
        }

        [Test]
        public void the_exception_is_an_argument_exception()
        {
            Assert.IsInstanceOf<ArgumentException>(_caught.InnerException);
        }
    }

    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_all_subscription_with_permissions : SpecificationWithConnection
    {
        private EventStorePersistentSubscription _sub;
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettingsBuilder.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromCurrent();
        protected override void When()
        {
            _conn.CreatePersistentSubscriptionForAllAsync("agroupname17", _settings, DefaultData.AdminCredentials).Wait();
            _sub = _conn.ConnectToPersistentSubscriptionForAll("agroupname17",
                (sub, e) => Console.Write("appeared"),
                (sub, reason, ex) => { }, DefaultData.AdminCredentials);
        }

        [Test]
        public void the_subscription_suceeds()
        {
            Assert.IsNotNull(_sub);
        }
    }

    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_all_subscription_without_permissions : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettingsBuilder.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromCurrent();

        protected override void When()
        {
            _conn.CreatePersistentSubscriptionForAllAsync("agroupname55", _settings,
                DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_subscription_fails_to_connect()
        {
            try
            {
                _conn.ConnectToPersistentSubscriptionForAll("agroupname55",
                    (sub, e) => Console.Write("appeared"),
                    (sub, reason, ex) => { });
                throw new Exception("should have thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<AggregateException>(ex);
                Assert.IsInstanceOf<AccessDeniedException>(ex.InnerException);
            }
        }
    }
*/

}
