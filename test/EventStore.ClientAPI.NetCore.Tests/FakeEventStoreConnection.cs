using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Eventstore.ClientAPI.Tests
{
    internal class FakeEventStoreConnection : IEventStoreConnection
    {
        private Func<Position, int, bool, UserCredentials, Task<AllEventsSlice>> _readAllEventsForwardAsync;
        private Func<string, int, int, Task<StreamEventsSlice>> _readStreamEventsForwardAsync;
        private Func<string, Action<EventStoreSubscription, ResolvedEvent>, Action<EventStoreSubscription, SubscriptionDropReason, Exception>, Task<EventStoreSubscription>> _subscribeToStreamAsync;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string ConnectionName { get; private set; }
        public ConnectionSettings Settings { get { return null; } }

        public Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public Task<DeleteResult> DeleteStreamAsync(string stream, int expectedVersion, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<DeleteResult> DeleteStreamAsync(string stream, int expectedVersion, bool hardDelete, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<WriteResult> AppendToStreamAsync(string stream, int expectedVersion, params EventData[] events)
        {
            throw new NotImplementedException();
        }

        public Task<WriteResult> AppendToStreamAsync(string stream, int expectedVersion, UserCredentials userCredentials, params EventData[] events)
        {
            throw new NotImplementedException();
        }

        public Task<WriteResult> AppendToStreamAsync(string stream, int expectedVersion, IEnumerable<EventData> events, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<EventStoreTransaction> StartTransactionAsync(string stream, int expectedVersion, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public EventStoreTransaction ContinueTransaction(long transactionId, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<EventReadResult> ReadEventAsync(string stream, int eventNumber, bool resolveLinkTos, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public void HandleReadStreamEventsForwardAsync(Func<string, int, int, Task<StreamEventsSlice>> callback)
        {
            _readStreamEventsForwardAsync = callback;
        }

        public Task<StreamEventsSlice> ReadStreamEventsForwardAsync(string stream, int start, int count, bool resolveLinkTos,
            UserCredentials userCredentials = null)
        {
            return _readStreamEventsForwardAsync(stream, start, count);
        }

        public Task<StreamEventsSlice> ReadStreamEventsBackwardAsync(string stream, int start, int count, bool resolveLinkTos,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public void HandleReadAllEventsForwardAsync(Func<Position, int, bool, UserCredentials, Task<AllEventsSlice>> callback)
        {
            _readAllEventsForwardAsync = callback;
        }

        public Task<AllEventsSlice> ReadAllEventsForwardAsync(Position position, int maxCount, bool resolveLinkTos,
            UserCredentials userCredentials = null)
        {
            return _readAllEventsForwardAsync(position, maxCount, resolveLinkTos, userCredentials);
        }

        public Task<AllEventsSlice> ReadAllEventsBackwardAsync(Position position, int maxCount, bool resolveLinkTos,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public void HandleSubscribeToStreamAsync(Func<string, Action<EventStoreSubscription, ResolvedEvent>, Action<EventStoreSubscription, SubscriptionDropReason, Exception>, Task<EventStoreSubscription>> callback)
        {
            _subscribeToStreamAsync = callback;
        }

        public Task<EventStoreSubscription> SubscribeToStreamAsync(string stream, bool resolveLinkTos, Action<EventStoreSubscription, ResolvedEvent> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
            UserCredentials userCredentials = null)
        {
            return _subscribeToStreamAsync(stream, eventAppeared, subscriptionDropped);
        }

        public EventStoreStreamCatchUpSubscription SubscribeToStreamFrom(string stream, int? lastCheckpoint, bool resolveLinkTos,
            Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
            UserCredentials userCredentials = null, int readBatchSize = 500)
        {
            throw new NotImplementedException();
        }

        public EventStoreStreamCatchUpSubscription SubscribeToStreamFrom(
            string stream,
            int? lastCheckpoint,
            CatchUpSubscriptionSettings settings,
            Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<EventStoreCatchUpSubscription> liveProcessingStarted = null,
            Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<EventStoreSubscription> SubscribeToAllAsync(bool resolveLinkTos, Action<EventStoreSubscription, ResolvedEvent> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public EventStorePersistentSubscriptionBase ConnectToPersistentSubscription(string stream, string groupName,
            Action<EventStorePersistentSubscriptionBase, ResolvedEvent> eventAppeared, Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null, int bufferSize = 10,
            bool autoAck = true)
        {
            throw new NotImplementedException();
        }

        public EventStoreAllCatchUpSubscription SubscribeToAllFrom(Position? lastCheckpoint, bool resolveLinkTos, Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null,
            int readBatchSize = 500)
        {
            throw new NotImplementedException();
        }

        public EventStoreAllCatchUpSubscription SubscribeToAllFrom(
            Position? lastCheckpoint,
            CatchUpSubscriptionSettings settings,
            Action<EventStoreCatchUpSubscription, ResolvedEvent> eventAppeared,
            Action<EventStoreCatchUpSubscription> liveProcessingStarted = null,
            Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePersistentSubscriptionAsync(string stream, string groupName, PersistentSubscriptionSettings settings,
            UserCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task CreatePersistentSubscriptionAsync(string stream, string groupName, PersistentSubscriptionSettings settings,
            UserCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task DeletePersistentSubscriptionAsync(string stream, string groupName, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<WriteResult> SetStreamMetadataAsync(string stream, int expectedMetastreamVersion, StreamMetadata metadata,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<WriteResult> SetStreamMetadataAsync(string stream, int expectedMetastreamVersion, byte[] metadata,
            UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<StreamMetadataResult> GetStreamMetadataAsync(string stream, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task<RawStreamMetadataResult> GetStreamMetadataAsRawBytesAsync(string stream, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public Task SetSystemSettingsAsync(SystemSettings settings, UserCredentials userCredentials = null)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ClientConnectionEventArgs> Connected;
        public event EventHandler<ClientConnectionEventArgs> Disconnected;
        public event EventHandler<ClientReconnectingEventArgs> Reconnecting;
        public event EventHandler<ClientClosedEventArgs> Closed;
        public event EventHandler<ClientErrorEventArgs> ErrorOccurred;
        public event EventHandler<ClientAuthenticationFailedEventArgs> AuthenticationFailed;

        protected virtual void OnErrorOccurred(ClientErrorEventArgs e)
        {
            var handler = ErrorOccurred;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnAuthenticationFailed(ClientAuthenticationFailedEventArgs e)
        {
            var handler = AuthenticationFailed;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnClosed(ClientClosedEventArgs e)
        {
            var handler = Closed;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnReconnecting(ClientReconnectingEventArgs e)
        {
            var handler = Reconnecting;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnDisconnected(ClientConnectionEventArgs e)
        {
            var handler = Disconnected;
            if (handler != null) handler(this, e);
        }

        public void OnConnected(ClientConnectionEventArgs e)
        {
            var handler = Connected;
            if (handler != null) handler(this, e);
        }

        public Task<EventStorePersistentSubscriptionBase> ConnectToPersistentSubscriptionAsync(string stream, string groupName, Action<EventStorePersistentSubscriptionBase, ResolvedEvent> eventAppeared, Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null, int bufferSize = 10, bool autoAck = true)
        {
            throw new NotImplementedException();
        }
    }
}