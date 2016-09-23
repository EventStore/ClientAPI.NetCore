using System.Threading;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture, Category("LongRunning")]
    public class subscribe_to_all_should
    {
        private const int Timeout = 10000;
        
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

        protected virtual IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [Test, Category("LongRunning")]
        public void allow_multiple_subscriptions()
        {
            const string stream = "subscribe_to_all_should_allow_multiple_subscriptions";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var appeared = new CountdownEvent(2);
                var dropped = new CountdownEvent(2);

                using (store.SubscribeToAllAsync(false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                using (store.SubscribeToAllAsync(false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                {
                    var create = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, TestEvent.NewTestEvent());
                    Assert.IsTrue(create.Wait(Timeout), "StreamCreateAsync timed out.");

                    Assert.IsTrue(appeared.Wait(Timeout), "Appeared countdown event timed out.");
                }
            }
        }

        [Test, Category("LongRunning")]
        public void catch_deleted_events_as_well()
        {
            const string stream = "subscribe_to_all_should_catch_created_and_deleted_events_as_well";
            using (var store = BuildConnection())
            {
                store.ConnectAsync().Wait();
                var appeared = new CountdownEvent(1);
                var dropped = new CountdownEvent(1);

                using (store.SubscribeToAllAsync(false, (s, x) => appeared.Signal(), (s, r, e) => dropped.Signal()).Result)
                {
                    var delete = store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true);
                    Assert.IsTrue(delete.Wait(Timeout), "DeleteStreamAsync timed out.");

                    Assert.IsTrue(appeared.Wait(Timeout), "Appeared countdown event didn't fire in time.");
                }
            }
        }
    }
}
