using EventStore.ClientAPI;

namespace Eventstore.ClientAPI.Tests.Helpers
{
    internal class OngoingTransaction
    {
        private readonly EventStoreTransaction _transaction;

        public OngoingTransaction(EventStoreTransaction transaction)
        {
            _transaction = transaction;
        }

        public OngoingTransaction Write(params EventData[] events)
        {
            _transaction.WriteAsync(events).Wait();
            return this;
        }

        public WriteResult Commit()
        {
            return _transaction.CommitAsync().Result;
        }
    }
}