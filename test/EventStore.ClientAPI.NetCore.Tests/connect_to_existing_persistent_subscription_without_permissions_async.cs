using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_without_permissions_async : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();

        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();

        private Exception _innerEx;

        protected override void When()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, "agroupname55", _settings,
                DefaultData.AdminCredentials).Wait();
            _innerEx = Assert.Throws<AggregateException>(() =>
            {
                _conn.ConnectToPersistentSubscriptionAsync(
                    _stream,
                    "agroupname55",
                    (sub, e) => Console.Write("appeared"),
                    (sub, reason, ex) => Console.WriteLine("dropped.")).Wait();
            }).InnerException;
        }

        [Test]
        public void the_subscription_fails_to_connect_with_access_denied_exception()
        {
            Assert.IsInstanceOf<AggregateException>(_innerEx);
            Assert.IsInstanceOf<AccessDeniedException>(_innerEx.InnerException);
        }
    }
}