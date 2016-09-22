using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class subscribe_to_all_catching_up_should
    {
        private static readonly ILogger Log = new ConsoleLogger();
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

        private IEventStoreConnection _conn;

        [SetUp]
        public void SetUp()
        {
            _conn = BuildConnection();
            _conn.ConnectAsync().Wait();
            _conn.SetStreamMetadataAsync("$all", ExpectedVersion.Any,
                                    StreamMetadata.Build().SetReadRole(SystemRoles.All),
                                    new UserCredentials(SystemUsers.Admin, SystemUsers.DefaultAdminPassword)).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            _conn.SetStreamMetadataAsync("$all", ExpectedVersion.Any,
                                    StreamMetadata.Build(),
                                    new UserCredentials(SystemUsers.Admin, SystemUsers.DefaultAdminPassword)).Wait();
            _conn.Close();
        }

        protected IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [Test, Category("LongRunning")]
        public void call_dropped_callback_after_stop_method_call()
        {
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var dropped = new CountdownEvent(1);
                var subscription = store.SubscribeToAllFrom(null,
                                                            CatchUpSubscriptionSettings.Default,
                                                            (x, y) => { },
                                                            _ => Log.Info("Live processing started."),
                                                            (x, y, z) => dropped.Signal());

                Assert.IsFalse(dropped.Wait(0));
                subscription.Stop(Timeout);
                Assert.IsTrue(dropped.Wait(Timeout));
            }
        }

        [Test, Category("LongRunning")]
        public void call_dropped_callback_when_an_error_occurs_while_processing_an_event()
        {
            const string stream = "all_call_dropped_callback_when_an_error_occurs_while_processing_an_event";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                store.AppendToStreamAsync(stream, ExpectedVersion.Any, new EventData(Guid.NewGuid(), "event", false, new byte[3], null)).Wait();

                var dropped = new CountdownEvent(1);
                store.SubscribeToAllFrom(null, CatchUpSubscriptionSettings.Default,
                                           (x, y) => { throw new Exception("Error"); },
                                           _ => Log.Info("Live processing started."),
                                           (x, y, z) => dropped.Signal());
                Assert.IsTrue(dropped.Wait(Timeout));
            }
        }

        [Test, Category("LongRunning"), Ignore("No way to guarantee an empty db")]
        public void be_able_to_subscribe_to_empty_db()
        {
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var appeared = new ManualResetEventSlim(false);
                var dropped = new CountdownEvent(1);

                var subscription = store.SubscribeToAllFrom(null,
                                                            CatchUpSubscriptionSettings.Default,
                                                            (_, x) =>
                                                            {
                                                                if (!SystemStreams.IsSystemStream(x.OriginalEvent.EventStreamId))
                                                                    appeared.Set();
                                                            },
                                                            _ => Log.Info("Live processing started."),
                                                            (_, __, ___) => dropped.Signal());

                Thread.Sleep(100); // give time for first pull phase
                store.SubscribeToAllAsync(false, (s, x) => { }, (s, r, e) => { }).Wait();
                Thread.Sleep(100);

                Assert.IsFalse(appeared.Wait(0), "Some event appeared.");
                Assert.IsFalse(dropped.Wait(0), "Subscription was dropped prematurely.");
                subscription.Stop(Timeout);
                Assert.IsTrue(dropped.Wait(Timeout));
            }
        }

        [Test, Category("LongRunning")]
        public void read_all_existing_events_and_keep_listening_to_new_ones()
        {
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var position = _conn.ReadAllEventsBackwardAsync(Position.End, 1, false).Result.NextPosition;
                var events = new List<ResolvedEvent>();
                var appeared = new CountdownEvent(20);
                var dropped = new CountdownEvent(1);

                for (int i = 0; i < 10; ++i)
                {
                    store.AppendToStreamAsync("all_read_all_existing_events_and_keep_listening_to_new_ones-" + i.ToString(), -1, new EventData(Guid.NewGuid(), "et-" + i.ToString(), false, new byte[3], null)).Wait();
                }

                var subscription = store.SubscribeToAllFrom(position,
                                                            CatchUpSubscriptionSettings.Default,
                                                            (x, y) =>
                                                            {
                                                                if (!SystemStreams.IsSystemStream(y.OriginalEvent.EventStreamId))
                                                                {
                                                                    events.Add(y);
                                                                    appeared.Signal();
                                                                }
                                                            },
                                                            _ => Log.Info("Live processing started."),
                                                            (x, y, z) => dropped.Signal());
                for (int i = 10; i < 20; ++i)
                {
                    store.AppendToStreamAsync("all_read_all_existing_events_and_keep_listening_to_new_ones-" + i.ToString(), -1, new EventData(Guid.NewGuid(), "et-" + i.ToString(), false, new byte[3], null)).Wait();
                }

                if (!appeared.Wait(Timeout))
                {
                    Assert.IsFalse(dropped.Wait(0), "Subscription was dropped prematurely.");
                    Assert.Fail("Could not wait for all events.");
                }

                Assert.AreEqual(20, events.Count);
                for (int i = 0; i < 20; ++i)
                {
                    Assert.AreEqual("et-" + i.ToString(), events[i].OriginalEvent.EventType);
                }

                Assert.IsFalse(dropped.Wait(0));
                subscription.Stop(Timeout);
                Assert.IsTrue(dropped.Wait(Timeout));
            }
        }

        [Test, Category("LongRunning"), Ignore("Not working against single db")]
        public void filter_events_and_keep_listening_to_new_ones()
        {
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var events = new List<ResolvedEvent>();
                var appeared = new CountdownEvent(10);
                var dropped = new CountdownEvent(1);

                for (int i = 0; i < 10; ++i)
                {
                    store.AppendToStreamAsync("all_filter_events_and_keep_listening_to_new_ones-" + i.ToString(), -1, new EventData(Guid.NewGuid(), "et-" + i.ToString(), false, new byte[3], null)).Wait();
                }

                var allSlice = store.ReadAllEventsForwardAsync(Position.Start, 100, false).Result;
                var lastEvent = allSlice.Events.Last();

                var subscription = store.SubscribeToAllFrom(lastEvent.OriginalPosition,
                                                            CatchUpSubscriptionSettings.Default,
                                                            (x, y) =>
                                                            {
                                                                events.Add(y);
                                                                appeared.Signal();
                                                            },
                                                            _ => Log.Info("Live processing started."),
                                                            (x, y, z) =>
                                                            {
                                                                Log.Info("Subscription dropped: {0}, {1}.", y, z);
                                                                dropped.Signal();
                                                            });

                for (int i = 10; i < 20; ++i)
                {
                    store.AppendToStreamAsync("all_filter_events_and_keep_listening_to_new_ones-" + i.ToString(), -1, new EventData(Guid.NewGuid(), "et-" + i.ToString(), false, new byte[3], null)).Wait();
                }
                Log.Info("Waiting for events...");
                if (!appeared.Wait(Timeout))
                {
                    Assert.IsFalse(dropped.Wait(0), "Subscription was dropped prematurely.");
                    Assert.Fail("Could not wait for all events.");
                }
                Log.Info("Events appeared...");
                Assert.AreEqual(10, events.Count);
                for (int i = 0; i < events.Count; ++i)
                {
                    Assert.AreEqual("et-" + (10 + i).ToString(), events[i].OriginalEvent.EventType);
                }

                Assert.IsFalse(dropped.Wait(0));
                subscription.Stop(Timeout);
                Assert.IsTrue(dropped.Wait(Timeout));

                Assert.AreEqual(events.Last().OriginalPosition, subscription.LastProcessedPosition);
            }
        }

        [Test, Category("LongRunning"), Ignore("not working against single db")]
        public void filter_events_and_work_if_nothing_was_written_after_subscription()
        {
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();

                var events = new List<ResolvedEvent>();
                var appeared = new CountdownEvent(1);
                var dropped = new CountdownEvent(1);

                for (int i = 0; i < 10; ++i)
                {
                    store.AppendToStreamAsync("all_filter_events_and_work_if_nothing_was_written_after_subscription-" + i.ToString(), -1, new EventData(Guid.NewGuid(), "et-" + i.ToString(), false, new byte[3], null)).Wait();
                }

                var allSlice = store.ReadAllEventsBackwardAsync(Position.End, 2, false).Result;
                var lastEvent = allSlice.Events[1];

                var subscription = store.SubscribeToAllFrom(lastEvent.OriginalPosition,
                                                            CatchUpSubscriptionSettings.Default,
                                                            (x, y) =>
                                                            {
                                                                events.Add(y);
                                                                appeared.Signal();
                                                            },
                                                            _ => Log.Info("Live processing started."),
                                                            (x, y, z) =>
                                                            {
                                                                Log.Info("Subscription dropped: {0}, {1}.", y, z);
                                                                dropped.Signal();
                                                            });

                Log.Info("Waiting for events...");
                if (!appeared.Wait(Timeout))
                {
                    Assert.IsFalse(dropped.Wait(0), "Subscription was dropped prematurely.");
                    Assert.Fail("Could not wait for all events.");
                }
                Log.Info("Events appeared...");
                Assert.AreEqual(1, events.Count);
                Assert.AreEqual("et-9", events[0].OriginalEvent.EventType);

                Assert.IsFalse(dropped.Wait(0));
                subscription.Stop(Timeout);
                Assert.IsTrue(dropped.Wait(Timeout));

                Assert.AreEqual(events.Last().OriginalPosition, subscription.LastProcessedPosition);
            }
        }
    }
}
