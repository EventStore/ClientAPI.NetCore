using EventStore.ClientAPI;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI.Helpers
{
    internal class StreamWriter
    {
        private readonly IEventStoreConnection _store;
        private readonly string _stream;
        private readonly long _version;

        public StreamWriter(IEventStoreConnection store, string stream, long version)
        {
            _store = store;
            _stream = stream;
            _version = version;
        }

        public TailWriter Append(params EventData[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                var expVer = _version == ExpectedVersion.Any ? ExpectedVersion.Any : _version + i;
                var nextExpVer = _store.AppendToStreamAsync(_stream, expVer, new[] { events[i] }).Result.NextExpectedVersion;
                if (_version != ExpectedVersion.Any)
                    Assert.AreEqual(expVer + 1, nextExpVer);
            }
            return new TailWriter(_store, _stream);
        }
    }

    //TODO GFY this should be removed and merged with the public idea of a transaction.
}
