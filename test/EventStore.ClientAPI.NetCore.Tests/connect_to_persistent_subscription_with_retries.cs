using EventStore.ClientAPI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class connect_to_persistent_subscription_with_retries : SpecificationWithConnection
    {
        private readonly string _stream = Guid.NewGuid().ToString("N");
        private readonly PersistentSubscriptionSettings _settings = PersistentSubscriptionSettings.Create()
                                                                .DoNotResolveLinkTos()
                                                                .StartFromBeginning();

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly Guid _id = Guid.NewGuid();
        int? _retryCount;
        private const string _group = "retries";

        protected override void Given()
        {
            _conn.CreatePersistentSubscriptionAsync(_stream, _group, _settings,
                DefaultData.AdminCredentials).Wait();
            _conn.ConnectToPersistentSubscription(
             _stream,
             _group,
             HandleEvent,
             (sub, reason, ex) => { },
             DefaultData.AdminCredentials,autoAck:false);

        }

        protected override void When()
        {
            _conn.AppendToStreamAsync(_stream, ExpectedVersion.Any, DefaultData.AdminCredentials,
                new EventData(_id, "test", true, Encoding.UTF8.GetBytes("{'foo' : 'bar'}"), new byte[0])).Wait();
        }

        private Task HandleEvent(EventStorePersistentSubscriptionBase sub, ResolvedEvent resolvedEvent, int? retryCount)
        {
            if (retryCount > 4)
            {
                _retryCount = retryCount;
                sub.Acknowledge(resolvedEvent);
                _resetEvent.Set();
            }
            else
            {
                sub.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Retry, "Not yet tried enough times");
            }
            return Task.CompletedTask;
        }

        [Test]
        public void events_are_retried_until_success()
        {
            Assert.IsTrue(_resetEvent.WaitOne(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(5, _retryCount);
        }
    }
}
