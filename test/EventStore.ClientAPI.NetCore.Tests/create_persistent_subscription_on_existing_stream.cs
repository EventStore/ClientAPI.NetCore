using System;
using System.Text;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_on_existing_stream : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        protected override void When()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "whatever", true, Encoding.UTF8.GetBytes("{'foo' : 2}"), new Byte[0]));
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.DoesNotThrow(
                () =>
                    _conn.CreatePersistentSubscriptionAsync(_stream, "existing", _settings, DefaultData.AdminCredentials)
                        .Wait());
        }
    }


    //ALL
/*

    [TestFixture, Category("LongRunning")]
    public class create_persistent_subscription_on_all : SpecificationWithConnection
    {
        private PersistentSubscriptionCreateResult _result;
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettingsBuilder.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromCurrent();

        protected override void When()
        {
            _result = _conn.CreatePersistentSubscriptionForAllAsync("group", _settings, DefaultData.AdminCredentials).Result;
        }

        [Test]
        public void the_completion_succeeds()
        {
            Assert.AreEqual(PersistentSubscriptionCreateStatus.Success, _result.Status);
        }
    }


    [TestFixture, Category("LongRunning")]
    public class create_duplicate_persistent_subscription_group_on_all : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettingsBuilder.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromCurrent();
        protected override void When()
        {
            _conn.CreatePersistentSubscriptionForAllAsync("group32", _settings, DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_completion_fails_with_invalid_operation_exception()
        {
            try
            {
                _conn.CreatePersistentSubscriptionForAllAsync("group32", _settings, DefaultData.AdminCredentials).Wait();
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
    public class create_persistent_subscription_group_on_all_without_permissions : SpecificationWithConnection
    {
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettingsBuilder.Create()
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
                _conn.CreatePersistentSubscriptionForAllAsync("group57", _settings, null).Wait();
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
