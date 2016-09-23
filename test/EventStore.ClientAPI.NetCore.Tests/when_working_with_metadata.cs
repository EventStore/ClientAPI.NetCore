using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Utils;
using NUnit.Framework;
using ExpectedVersion = EventStore.ClientAPI.ExpectedVersion;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class when_working_with_metadata 
    {
        private IEventStoreConnection _connection;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

            _connection = BuildConnection();
            _connection.ConnectAsync().Wait();
        }

        protected virtual IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _connection.Close();
        }

        [Test]
        public void when_getting_metadata_for_an_existing_stream_and_no_metadata_exists()
        {
            const string stream = "when_getting_metadata_for_an_existing_stream_and_no_metadata_exists";

            _connection.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Wait();

            var meta = _connection.GetStreamMetadataAsRawBytesAsync(stream).Result;
            Assert.AreEqual(stream, meta.Stream);
            Assert.AreEqual(false, meta.IsStreamDeleted);
            Assert.AreEqual(-1, meta.MetastreamVersion);
            Assert.AreEqual(Helper.UTF8NoBom.GetBytes(""), meta.StreamMetadata);
        }
    }
}
