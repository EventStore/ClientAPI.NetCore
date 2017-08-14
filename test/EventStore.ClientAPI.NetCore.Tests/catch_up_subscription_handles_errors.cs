using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.ClientOperations;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Internal;
using NUnit.Framework;
using ClientMessage = EventStore.ClientAPI.Messages.ClientMessage;
using ResolvedEvent = EventStore.ClientAPI.ResolvedEvent;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture]
    public class catch_up_subscription_handles_errors
    {
        private static int TimeoutMs = 2000;
        private FakeEventStoreConnection _connection;
        private IList<ResolvedEvent> _raisedEvents;
        private bool _liveProcessingStarted;
        private bool _isDropped;
        private ManualResetEventSlim _dropEvent;
        private ManualResetEventSlim _raisedEventEvent;
        private Exception _dropException;
        private SubscriptionDropReason _dropReason;
        private EventStoreStreamCatchUpSubscription _subscription;
        private static readonly string StreamId = "stream1";

        [SetUp]
        public void SetUp()
        {
            _connection = new FakeEventStoreConnection();
            _raisedEvents = new List<ResolvedEvent>();
            _dropEvent = new ManualResetEventSlim();
            _raisedEventEvent = new ManualResetEventSlim();
            _liveProcessingStarted = false;
            _isDropped = false;
            _dropReason = SubscriptionDropReason.Unknown;
            _dropException = null;

            var settings = new CatchUpSubscriptionSettings(1, 1, false, false);
            _subscription = new EventStoreStreamCatchUpSubscription(_connection, new NoopLogger(), StreamId, null, null,
                (subscription, ev) =>
                {
                    _raisedEvents.Add(ev);
                    _raisedEventEvent.Set();
                    return Task.CompletedTask;
                },
                subscription =>
                {
                    _liveProcessingStarted = true;
                },
                (subscription, reason, ex) =>
                {
                    _isDropped = true;
                    _dropReason = reason;
                    _dropException = ex;
                    _dropEvent.Set();
                },
                settings);
        }

        [Test]
        public void read_events_til_stops_subscription_when_throws_immediately()
        {

            var expectedException = new Exception("Test");

            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
                                                           {
                                                               Assert.That(stream, Is.EqualTo(StreamId));
                                                               Assert.That(start, Is.EqualTo(0));
                                                               Assert.That(max, Is.EqualTo(1));
                                                               throw expectedException;
                                                           });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
        }

        private void AssertStartFailsAndDropsSubscriptionWithException(Exception expectedException)
        {
            Assert.That(() => _subscription.StartAsync().Wait(TimeoutMs), Throws.TypeOf<AggregateException>());
            Assert.That(_isDropped);
            Assert.That(_dropReason, Is.EqualTo(SubscriptionDropReason.CatchUpError));
            Assert.That(_dropException, Is.SameAs(expectedException));
            Assert.That(_liveProcessingStarted, Is.False);
        }

        [Test]
        public void read_events_til_stops_subscription_when_throws_asynchronously()
        {
         
            var expectedException = new Exception("Test");
            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                Assert.That(stream, Is.EqualTo(StreamId));
                Assert.That(start, Is.EqualTo(0));
                Assert.That(max, Is.EqualTo(1));
                taskCompletionSource.SetException(expectedException);
                return taskCompletionSource.Task;
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
        }

        [Test]
        public void read_events_til_stops_subscription_when_second_read_throws_immediately()
        {
            var expectedException = new Exception("Test");

            int callCount = 0;

            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                Assert.That(stream, Is.EqualTo(StreamId));
                Assert.That(max, Is.EqualTo(1));

                if (callCount++ == 0)
                {
                    Assert.That(start, Is.EqualTo(0));
                    var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                    taskCompletionSource.SetResult(CreateStreamEventsSlice());
                    return taskCompletionSource.Task;

                }
                else
                {
                    Assert.That(start, Is.EqualTo(1));
                    throw expectedException;    
                }
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
            Assert.That(_raisedEvents.Count, Is.EqualTo(1));
        }

        [Test]
        public void read_events_til_stops_subscription_when_second_read_throws_asynchronously()
        {
            var expectedException = new Exception("Test");

            int callCount = 0;

            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                Assert.That(stream, Is.EqualTo(StreamId));
                Assert.That(max, Is.EqualTo(1));

                if (callCount++ == 0)
                {
                    Assert.That(start, Is.EqualTo(0));
                    var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                    taskCompletionSource.SetResult(CreateStreamEventsSlice());
                    return taskCompletionSource.Task;

                }
                else
                {
                    Assert.That(start, Is.EqualTo(1));
                    var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                    taskCompletionSource.SetException(expectedException);
                    return taskCompletionSource.Task;
                }
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
            Assert.That(_raisedEvents.Count, Is.EqualTo(1));
        }       

        [Test]
        public void start_stops_subscription_if_subscribe_fails_immediately()
        {
            var expectedException = new Exception("Test");

            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                taskCompletionSource.SetResult(CreateStreamEventsSlice(isEnd: true));
                return taskCompletionSource.Task;
            });

            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                Assert.That(stream, Is.EqualTo(StreamId));
                throw expectedException;
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
            Assert.That(_raisedEvents.Count, Is.EqualTo(1));
        }

        [Test]
        public void start_stops_subscription_if_subscribe_fails_async()
        {
            var expectedException = new Exception("Test");

            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                taskCompletionSource.SetResult(CreateStreamEventsSlice(isEnd: true));
                return taskCompletionSource.Task;
            });

            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                var taskCompletionSource = new TaskCompletionSource<EventStoreSubscription>();
                taskCompletionSource.SetException(expectedException);
                return taskCompletionSource.Task;
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
            Assert.That(_raisedEvents.Count, Is.EqualTo(1));
        }

        [Test]
        public void start_stops_subscription_if_historical_missed_events_load_fails_immediate()
        {
            var expectedException = new Exception("Test");

            int callCount = 0;
            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                if (callCount++ == 0)
                {
                    var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(isEnd: true));
                    return taskCompletionSource.Task;
                }
                else
                {
                    Assert.That(start, Is.EqualTo(1));
                    throw expectedException;
                }
            });

            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                var taskCompletionSource = new TaskCompletionSource<EventStoreSubscription>();
                taskCompletionSource.SetResult(CreateVolatileSubscription(raise, drop, 1));
                return taskCompletionSource.Task;
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
            Assert.That(_raisedEvents.Count, Is.EqualTo(1));
        }


        [Test]
        public void start_stops_subscription_if_historical_missed_events_load_fails_async()
        {
            var expectedException = new Exception("Test");

            int callCount = 0;
            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                if (callCount++ == 0)
                {
                    var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(isEnd: true));
                    return taskCompletionSource.Task;
                }
                else
                {
                    Assert.That(start, Is.EqualTo(1));
                    var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                    taskCompletionSource.SetException(expectedException);
                    return taskCompletionSource.Task;
                }
            });

            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                var taskCompletionSource = new TaskCompletionSource<EventStoreSubscription>();
                taskCompletionSource.SetResult(CreateVolatileSubscription(raise, drop, 1));
                return taskCompletionSource.Task;
            });

            AssertStartFailsAndDropsSubscriptionWithException(expectedException);
            Assert.That(_raisedEvents.Count, Is.EqualTo(1));
        }

        [Test]
        public void start_completes_onces_subscription_is_live()
        {
            var finalEvent = new ManualResetEventSlim();
            int callCount = 0;
            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                callCount++;

                var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                if (callCount == 1)
                {
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(isEnd: true));
                }
                else if (callCount == 2)
                {
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(fromEvent:1, isEnd: true));
                    Assert.That(finalEvent.Wait(TimeoutMs));
                }
                
                return taskCompletionSource.Task;
            });

            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                var taskCompletionSource = new TaskCompletionSource<EventStoreSubscription>();
                taskCompletionSource.SetResult(CreateVolatileSubscription(raise, drop, 1));
                return taskCompletionSource.Task;
            });

            var task = _subscription.StartAsync();

            Assert.That(task.Status, Is.Not.EqualTo(TaskStatus.RanToCompletion));
            
            finalEvent.Set();

            Assert.That(task.Wait(TimeoutMs));
        }

        [Test]
        public void when_live_processing_and_disconnected_reconnect_keeps_events_ordered()
        {
            int callCount = 0;
            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                callCount++;

                var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                if (callCount == 1)
                {
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(fromEvent: 0, count: 0, isEnd: true));
                }
                else if (callCount == 2)
                {
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(fromEvent: 0, count: 0, isEnd: true));
                }

                return taskCompletionSource.Task;
            });

            VolatileEventStoreSubscription volatileEventStoreSubscription = null;
            Action<EventStoreSubscription, SubscriptionDropReason, Exception> innerSubscriptionDrop = null;
            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                innerSubscriptionDrop = drop;
                var taskCompletionSource = new TaskCompletionSource<EventStoreSubscription>();
                volatileEventStoreSubscription = CreateVolatileSubscription(raise, drop, null);
                taskCompletionSource.SetResult(volatileEventStoreSubscription);
                return taskCompletionSource.Task;
            });

            Assert.That(_subscription.StartAsync().Wait(TimeoutMs));
            Assert.That(_raisedEvents.Count, Is.EqualTo(0));

            Assert.That(innerSubscriptionDrop, Is.Not.Null);
            innerSubscriptionDrop(volatileEventStoreSubscription, SubscriptionDropReason.ConnectionClosed, null);

            Assert.That(_dropEvent.Wait(TimeoutMs));
            _dropEvent.Reset();

            var waitForOutOfOrderEvent = new ManualResetEventSlim();
            callCount = 0;
            _connection.HandleReadStreamEventsForwardAsync((stream, start, max) =>
            {
                callCount++;

                var taskCompletionSource = new TaskCompletionSource<StreamEventsSlice>();
                if (callCount == 1)
                {
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(fromEvent: 0, count:0, isEnd: true));
                }
                else if (callCount == 2)
                {
                    Assert.That(waitForOutOfOrderEvent.Wait(TimeoutMs));
                    taskCompletionSource.SetResult(CreateStreamEventsSlice(fromEvent: 0, count: 1, isEnd: true));
                }

                return taskCompletionSource.Task;
            });

            var event1 = new ClientMessage.ResolvedEvent(new ClientMessage.EventRecord(StreamId, 1, Guid.NewGuid().ToByteArray(), null, 0, 0, null, null, null, null), null, 0, 0);
            
            _connection.HandleSubscribeToStreamAsync((stream, raise, drop) =>
            {
                var taskCompletionSource = new TaskCompletionSource<EventStoreSubscription>();
                VolatileEventStoreSubscription volatileEventStoreSubscription2 = CreateVolatileSubscription(raise, drop, null);
                taskCompletionSource.SetResult(volatileEventStoreSubscription);

                raise(volatileEventStoreSubscription2, new ResolvedEvent(event1));
                
                return taskCompletionSource.Task;
            });


            var reconnectTask = Task.Factory.StartNew(() =>
            {
                _connection.OnConnected(new ClientConnectionEventArgs(_connection, new IPEndPoint(IPAddress.Any, 1)));

            }, TaskCreationOptions.AttachedToParent);

            Assert.That(_raisedEventEvent.Wait(100), Is.False);

            waitForOutOfOrderEvent.Set();

            Assert.That(_raisedEventEvent.Wait(TimeoutMs));

            Assert.That(_raisedEvents[0].OriginalEventNumber, Is.EqualTo(0));
            Assert.That(_raisedEvents[1].OriginalEventNumber, Is.EqualTo(1));

            Assert.That(reconnectTask.Wait(TimeoutMs));
        }

        private static VolatileEventStoreSubscription CreateVolatileSubscription(Func<EventStoreSubscription, ResolvedEvent, Task> raise, Action<EventStoreSubscription, SubscriptionDropReason, Exception> drop, int? lastEventNumber)
        {
            return new VolatileEventStoreSubscription(new VolatileSubscriptionOperation(new NoopLogger(), new TaskCompletionSource<EventStoreSubscription>(), StreamId, false, null, raise, drop, false, () => null), StreamId, -1, lastEventNumber);
        }

        private static StreamEventsSlice CreateStreamEventsSlice(int fromEvent = 0, int count = 1, bool isEnd = false)
        {
            var events = Enumerable.Range(0, count)
                .Select(
                    i =>
                        new ClientMessage.ResolvedIndexedEvent(
                            new ClientMessage.EventRecord(StreamId, i, Guid.NewGuid().ToByteArray(), null, 0, 0, null,
                                null, null, null), null))
                .ToArray();

            return new StreamEventsSlice(SliceReadStatus.Success, StreamId, fromEvent, ReadDirection.Forward, events, fromEvent + count, 100, isEnd);
        }
    }
}
