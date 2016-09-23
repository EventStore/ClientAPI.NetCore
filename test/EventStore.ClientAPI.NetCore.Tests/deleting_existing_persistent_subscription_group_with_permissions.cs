using System;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class deleting_existing_persistent_subscription_group_with_permissions : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
                                                                        .DoNotResolveLinkTos()
                                                                        .StartFromCurrent();
        private readonly string _stream = Guid.NewGuid().ToString();

        protected override void When()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, "groupname123", _settings,
                DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_delete_of_group_succeeds()
        {
             Assert.DoesNotThrow(() => _conn.DeletePersistentSubscriptionAsync(_stream, "groupname123", DefaultData.AdminCredentials).Wait());
        }
    }


    //ALL
/*

    [TestFixture, Category("LongRunning")]
    public class deleting_existing_persistent_subscription_group_on_all_with_permissions : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettingsBuilder.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromCurrent();
        protected override void When()
        {
            _conn.CreatePersistentSubscriptionForAllAsync("groupname123", _settings,
                DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_delete_of_group_succeeds()
        {
            var result = _conn.DeletePersistentSubscriptionForAllAsync("groupname123", DefaultData.AdminCredentials).Result;
            Assert.AreEqual(PersistentSubscriptionDeleteStatus.Success, result.Status);
        }
    }

    [TestFixture, Category("LongRunning")]
    public class deleting_persistent_subscription_group_on_all_that_doesnt_exist : SpecificationWithConnection
    {
        protected override void When()
        {
        }

        [Test]
        public void the_delete_fails_with_argument_exception()
        {
            try
            {
                _conn.DeletePersistentSubscriptionForAllAsync(Guid.NewGuid().ToString(), DefaultData.AdminCredentials).Wait();
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


    [TestFixture, Category("LongRunning")]
    public class deleting_persistent_subscription_group_on_all_without_permissions : SpecificationWithConnection
    {
        protected override void When()
        {
        }

        [Test]
        public void the_delete_fails_with_access_denied()
        {
            try
            {
                _conn.DeletePersistentSubscriptionForAllAsync(Guid.NewGuid().ToString()).Wait();
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
*/

}
