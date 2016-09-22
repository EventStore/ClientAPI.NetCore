using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.ClientOperations;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_with_max_one_client : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent()
            .WithMaxSubscriberCountOf(1);

        private Exception _exception;

        private const string _group = "startinbeginning1";

        protected override void Given()
        {
            base.Given();
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscription(
                _stream,
                _group,
                (s, e) => s.Acknowledge(e),
                (sub, reason, ex) => { },
                DefaultData.AdminCredentials);
        }

        protected override void When()
        {
            _exception = Assert.Throws<AggregateException>(() => {
                _conn.ConnectToPersistentSubscription(
                    _stream,
                    _group,
                    (s, e) => s.Acknowledge(e),
                    (sub, reason, ex) => { },
                    DefaultData.AdminCredentials);
                throw new Exception("should have thrown.");
            }).InnerException;
        }

        [Test]
        public void the_second_subscription_fails_to_connect()
        {
            Assert.IsInstanceOf<AggregateException>(_exception);
            Assert.IsInstanceOf<MaximumSubscribersReachedException>(_exception.InnerException);
        }
    }
}