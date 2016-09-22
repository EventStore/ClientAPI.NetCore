using System.Threading;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class subscribe_should
    {
        private const int Timeout = 10000;

        protected IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [Test, Category("LongRunning")]
        public void be_able_to_subscribe_to_non_existing_stream_and_then_catch_new_event()
        {
            const string stream = "subscribe_should_be_able_to_subscribe_to_non_existing_stream_and_then_catch_created_event";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var appeared = new CountdownEvent(1);
                var dropped = new CountdownEvent(1);

                using (store.SubscribeToStreamAsync(stream, false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                {
                    store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Wait();
                    Assert.IsTrue(appeared.Wait(Timeout), "Appeared countdown event timed out.");
                }
            }
        }

        [Test, Category("LongRunning")]
        public void allow_multiple_subscriptions_to_same_stream()
        {
            const string stream = "subscribe_should_allow_multiple_subscriptions_to_same_stream";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var appeared = new CountdownEvent(2);
                var dropped = new CountdownEvent(2);

                using (store.SubscribeToStreamAsync(stream, false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                using (store.SubscribeToStreamAsync(stream, false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                {
                    store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Wait();
                    Assert.IsTrue(appeared.Wait(Timeout), "Appeared countdown event timed out.");
                }
            }
        }

        [Test, Category("LongRunning")]
        public void call_dropped_callback_after_unsubscribe_method_call()
        {
            const string stream = "subscribe_should_call_dropped_callback_after_unsubscribe_method_call";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var dropped = new CountdownEvent(1);
                using (var subscription = store.SubscribeToStreamAsync(stream, false, (s, x) => { }, (s, r, e) => dropped.Signal()).Result)
                {
                    subscription.Unsubscribe();
                }
                Assert.IsTrue(dropped.Wait(Timeout), "Dropped countdown event timed out.");
            }
        }

        [Test, Category("LongRunning")]
        public void catch_deleted_events_as_well()
        {
            const string stream = "subscribe_should_catch_created_and_deleted_events_as_well";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var appeared = new CountdownEvent(1);
                var dropped = new CountdownEvent(1);
                using (store.SubscribeToStreamAsync(stream, false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                {
                    store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true).Wait();
                    Assert.IsTrue(appeared.Wait(Timeout), "Appeared countdown event timed out.");
                }
            }
        }
    }
}
