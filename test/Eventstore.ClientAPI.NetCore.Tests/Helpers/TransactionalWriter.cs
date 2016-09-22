using EventStore.ClientAPI;

namespace Eventstore.ClientAPI.Tests.Helpers
{
    internal class TransactionalWriter
    {
        private readonly IEventStoreConnection _store;
        private readonly string _stream;

        public TransactionalWriter(IEventStoreConnection store, string stream)
        {
            _store = store;
            _stream = stream;
        }

        public OngoingTransaction StartTransaction(int expectedVersion)
        {
            return new OngoingTransaction(_store.StartTransactionAsync(_stream, expectedVersion).Result);
        }
    }
}