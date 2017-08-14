using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.ClientOperations;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_max_one_client_async : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent()
            .WithMaxSubscriberCountOf(1);

        private Exception _innerEx;

        private const string _group = "startinbeginning1";
        private EventStorePersistentSubscriptionBase _firstConn;

        protected override void Given()
        {
            base.Given();
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();
            // First connection
            _firstConn = _conn.ConnectToPersistentSubscriptionAsync(
                _stream,
                _group,
                (s, e) => s.Acknowledge(e),
                (sub, reason, ex) => { },
                DefaultData.AdminCredentials).Result;
        }

        protected override void When()
        {
            _innerEx = Assert.Throws<AggregateException>(() =>
            {
                // Second connection
                _conn.ConnectToPersistentSubscriptionAsync(
                    _stream,
                    _group,
                    (s, e) => s.Acknowledge(e),
                    (sub, reason, ex) => { },
                    DefaultData.AdminCredentials).Wait();
            }).InnerException;
        }

        [Test]
        public void the_first_subscription_connects_successfully()
        {
            Assert.IsNotNull(_firstConn);
        }

        [Test]
        public void the_second_subscription_throws_maximum_subscribers_reached_exception()
        {
            Assert.IsInstanceOf<AggregateException>(_innerEx);
            Assert.IsInstanceOf<MaximumSubscribersReachedException>(_innerEx.InnerException);
        }
    }
}