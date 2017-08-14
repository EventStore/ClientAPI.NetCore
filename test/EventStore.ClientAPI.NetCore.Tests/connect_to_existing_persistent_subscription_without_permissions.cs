using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_existing_persistent_subscription_without_permissions : SpecificationWithConnection
    {
        private readonly string _stream = "$" + Guid.NewGuid();
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
            .DoNotResolveLinkTos()
            .StartFromCurrent();
        protected override void When()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, "agroupname55", _settings,
                DefaultData.AdminCredentials).Wait();
        }

        [Test]
        public void the_subscription_fails_to_connect()
        {
            try
            {
                _conn.ConnectToPersistentSubscription( 
                    _stream,
                    "agroupname55",
                    (sub, e) => Console.Write("appeared"),
                    (sub, reason, ex) => Console.WriteLine("dropped."));
                throw new Exception("should have thrown.");
            }
            catch (Exception ex)
            {
                var innerEx = ex.InnerException;
                Assert.IsInstanceOf<AggregateException>(innerEx);
                Assert.IsInstanceOf<AccessDeniedException>(innerEx.InnerException);
            }
        }
    }
}