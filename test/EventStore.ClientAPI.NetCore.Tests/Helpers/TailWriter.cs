using EventStore.ClientAPI;

namespace Eventstore.ClientAPI.Tests.Helpers
{
    internal class TailWriter
    {
        private readonly IEventStoreConnection _store;
        private readonly string _stream;

        public TailWriter(IEventStoreConnection store, string stream)
        {
            _store = store;
            _stream = stream;
        }

        public TailWriter Then(EventData @event, long expectedVersion)
        {
            _store.AppendToStreamAsync(_stream, expectedVersion, new[] {@event}).Wait();
            return this;
        }
    }
}