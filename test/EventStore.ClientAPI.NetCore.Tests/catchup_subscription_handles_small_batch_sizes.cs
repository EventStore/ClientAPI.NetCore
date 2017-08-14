using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.Core.Tests.ClientAPI.Helpers;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Ignore("Very long running")]
    [Category("LongRunning")]
    public class catchup_subscription_handles_small_batch_sizes 
    {
        private string _streamName = "TestStream";
        private CatchUpSubscriptionSettings _settings;
        private IEventStoreConnection _conn;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _conn = BuildConnection();
            _conn.ConnectAsync().Wait();
            //Create 80000 events
            for(var i = 0; i < 80; i++)
            {
                _conn.AppendToStreamAsync(_streamName, ExpectedVersion.Any, CreateThousandEvents()).Wait();
            }

            _settings = new CatchUpSubscriptionSettings(100, 1, false, true);
        }

        private EventData[] CreateThousandEvents()
        {
            var events = new List<EventData>();
            for(var i = 0; i < 1000; i++)
            {
                events.Add(new EventData(Guid.NewGuid(), "testEvent", true, Encoding.UTF8.GetBytes("{ \"Foo\":\"Bar\" }"), null));
            }
            return events.ToArray();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _conn.Dispose();
        }

        protected virtual IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [Test]
        public void CatchupSubscriptionToAllHandlesManyEventsWithSmallBatchSize()
        {
            var mre = new ManualResetEvent(false);
            _conn.SubscribeToAllFrom(null, _settings, (sub, evnt) => {
                if(evnt.OriginalEventNumber % 1000 == 0)
                {
                    Console.WriteLine("Processed {0} events", evnt.OriginalEventNumber);
                }
            }, (sub) => { mre.Set(); }, null, new UserCredentials("admin", "changeit"));

            if (!mre.WaitOne(TimeSpan.FromMinutes(10)))
                Assert.Fail("Timed out waiting for test to complete");
        }

        [Test]
        public void CatchupSubscriptionToStreamHandlesManyEventsWithSmallBatchSize()
        {
            var mre = new ManualResetEvent(false);
            _conn.SubscribeToStreamFrom(_streamName, null, _settings, (sub, evnt) => {
                if (evnt.OriginalEventNumber % 1000 == 0)
                {
                    Console.WriteLine("Processed {0} events", evnt.OriginalEventNumber);
                }
            }, (sub) => { mre.Set(); }, null, new UserCredentials("admin", "changeit"));

            if (!mre.WaitOne(TimeSpan.FromMinutes(10)))
                Assert.Fail("Timed out waiting for test to complete");
        }
    }
}
