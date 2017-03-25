using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.UserManagement;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests.Security
{
    public abstract class AuthenticationTestBase
    {
        private readonly UserCredentials _userCredentials;
        protected IEventStoreConnection Connection;
        private readonly string _streamPrefix;
        protected AuthenticationTestBase(UserCredentials userCredentials = null)
        {
            _userCredentials = userCredentials;
            _streamPrefix = GetType().FullName;
        }

        private IEventStoreConnection SetupConnection()
        {
            return TestConnection.Create(TcpType.Normal, _userCredentials);
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {

            Connection = SetupConnection();
            Connection.ConnectAsync().Wait();
            var manager = new UsersManager(new NoopLogger(), TestNode.HttpEndPoint, TimeSpan.FromSeconds(10));
            CreateUsers(manager);



            Connection.SetStreamMetadataAsync(AdjustStreamId("noacl-stream"), ExpectedVersion.Any, StreamMetadata.Build())
                .Wait();
            Connection.SetStreamMetadataAsync(
                AdjustStreamId("read-stream"),
                ExpectedVersion.Any,
                StreamMetadata.Build().SetReadRole("user1")).Wait();
            Connection.SetStreamMetadataAsync(
                AdjustStreamId("write-stream"),
                ExpectedVersion.Any,
                StreamMetadata.Build().SetWriteRole("user1")).Wait();
            Connection.SetStreamMetadataAsync(
                AdjustStreamId("metaread-stream"),
                ExpectedVersion.Any,
                StreamMetadata.Build().SetMetadataReadRole("user1")).Wait();
            Connection.SetStreamMetadataAsync(
                AdjustStreamId("metawrite-stream"),
                ExpectedVersion.Any,
                StreamMetadata.Build().SetMetadataWriteRole("user1")).Wait();

            Connection.SetStreamMetadataAsync(
                AdjustStreamId("$all"),
                ExpectedVersion.Any,
                StreamMetadata.Build().SetReadRole("user1"),
                new UserCredentials("adm", "admpa$$")).Wait();

            Connection.SetStreamMetadataAsync(
                AdjustStreamId("$system-acl"),
                ExpectedVersion.Any,
                StreamMetadata.Build()
                    .SetReadRole("user1")
                    .SetWriteRole("user1")
                    .SetMetadataReadRole("user1")
                    .SetMetadataWriteRole("user1"),
                new UserCredentials("adm", "admpa$$")).Wait();
            Connection.SetStreamMetadataAsync(
                AdjustStreamId("$system-adm"),
                ExpectedVersion.Any,
                StreamMetadata.Build()
                    .SetReadRole(SystemRoles.Admins)
                    .SetWriteRole(SystemRoles.Admins)
                    .SetMetadataReadRole(SystemRoles.Admins)
                    .SetMetadataWriteRole(SystemRoles.Admins),
                new UserCredentials("adm", "admpa$$")).Wait();

            Connection.SetStreamMetadataAsync(
                AdjustStreamId("normal-all"),
                ExpectedVersion.Any,
                StreamMetadata.Build()
                    .SetReadRole(SystemRoles.All)
                    .SetWriteRole(SystemRoles.All)
                    .SetMetadataReadRole(SystemRoles.All)
                    .SetMetadataWriteRole(SystemRoles.All)).Wait();
            Connection.SetStreamMetadataAsync(
                AdjustStreamId("$system-all"),
                ExpectedVersion.Any,
                StreamMetadata.Build()
                    .SetReadRole(SystemRoles.All)
                    .SetWriteRole(SystemRoles.All)
                    .SetMetadataReadRole(SystemRoles.All)
                    .SetMetadataWriteRole(SystemRoles.All),
                new UserCredentials("adm", "admpa$$")).Wait();
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            Connection.SetStreamMetadataAsync("$all", ExpectedVersion.Any, StreamMetadata.Build(),
                TestNode.AdminCredentials).Wait();
            Connection.SetSystemSettingsAsync(new SystemSettings(null, null), TestNode.AdminCredentials).Wait();
        }

        private static bool CreateUsers(UsersManager manager)
        {
            var users = manager.ListAllAsync().Result;

            return 
                    CreateUserIfNotExists(manager, users, "user1", "Test User 1", new string[0], "pa$$1") &&
                    CreateUserIfNotExists(manager, users, "user2","Test User 2", new string[0], "pa$$2") &&
                    CreateUserIfNotExists(manager, users, "adm", "Administrator User", new[] {SystemRoles.Admins}, "admpa$$");
        }

        private static bool CreateUserIfNotExists(UsersManager manager, List<UserDetails> users, string loginName, string fullName,
            string[] groups, string password)
        {
            if (users.All(x => x.LoginName != loginName))
            {
                manager.CreateUserAsync(loginName,
                    fullName,
                    groups,
                    password, TestNode.AdminCredentials).Wait();
                return true;
            }
            return false;
        }

        string AdjustStreamId(string stream)
        {
            if (stream == "$all"  || (TestContext.CurrentContext != null && (stream.StartsWith("$"+ TestContext.CurrentContext.Test.Name) || stream.StartsWith(TestContext.CurrentContext.Test.Name)))) return stream;
            if (stream.StartsWith("$")) return $"${_streamPrefix}{stream.Substring(1)}";
            return _streamPrefix + stream;
        }
        protected void ReadEvent(string streamId, string login, string password)
        {
            Connection.ReadEventAsync(AdjustStreamId(streamId), -1, false,
                                 login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected void ReadStreamForward(string streamId, string login, string password)
        {
            Connection.ReadStreamEventsForwardAsync(AdjustStreamId(streamId), 0, 1, false,
                                               login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected void ReadStreamBackward(string streamId, string login, string password)
        {
            Connection.ReadStreamEventsBackwardAsync(AdjustStreamId(streamId), 0, 1, false,
                                                login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected void WriteStream(string streamId, string login, string password)
        {
            Connection.AppendToStreamAsync(AdjustStreamId(streamId), ExpectedVersion.Any, CreateEvents(),
                                      login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected EventStoreTransaction TransStart(string streamId, string login, string password)
        {
            return Connection.StartTransactionAsync(AdjustStreamId(streamId), ExpectedVersion.Any,
                                                login == null && password == null ? null : new UserCredentials(login, password))
            .Result;
        }

        protected void ReadAllForward(string login, string password)
        {
            Connection.ReadAllEventsForwardAsync(Position.Start, 1, false,
                                            login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected void ReadAllBackward(string login, string password)
        {
            Connection.ReadAllEventsBackwardAsync(Position.End, 1, false,
                                             login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected void ReadMeta(string streamId, string login, string password)
        {
            Connection.GetStreamMetadataAsRawBytesAsync(AdjustStreamId(streamId), login == null && password == null ? null : new UserCredentials(login, password)).Wait();
        }

        protected void WriteMeta(string streamId, string login, string password, string metawriteRole)
        {
            Connection.SetStreamMetadataAsync(AdjustStreamId(streamId), ExpectedVersion.Any,
                                         metawriteRole == null
                                            ? StreamMetadata.Build()
                                            : StreamMetadata.Build().SetReadRole(metawriteRole)
                                                                    .SetWriteRole(metawriteRole)
                                                                    .SetMetadataReadRole(metawriteRole)
                                                                    .SetMetadataWriteRole(metawriteRole),
                                         login == null && password == null ? null : new UserCredentials(login, password))
            .Wait();
        }

        protected void SubscribeToStream(string streamId, string login, string password)
        {
            using (Connection.SubscribeToStreamAsync(AdjustStreamId(streamId), false, (x, y) => { }, (x, y, z) => { },
                                                login == null && password == null ? null : new UserCredentials(login, password)).Result)
            {
            }
        }

        protected void SubscribeToAll(string login, string password)
        {
            using (Connection.SubscribeToAllAsync(false, (x, y) => { }, (x, y, z) => { },
                                             login == null && password == null ? null : new UserCredentials(login, password)).Result)
            {
            }
        }

        protected string CreateStreamWithMeta(StreamMetadata metadata, string streamPrefix = null)
        {
            var stream = (streamPrefix ?? string.Empty) + TestContext.CurrentContext.Test.Name;
            Connection.SetStreamMetadataAsync(stream, ExpectedVersion.NoStream,
                                         metadata, new UserCredentials("adm", "admpa$$")).Wait();
            return stream;
        }

        protected void DeleteStream(string streamId, string login, string password)
        {
            Connection.DeleteStreamAsync(AdjustStreamId(streamId), ExpectedVersion.Any, true,
                                    login == null && password == null ? null : new UserCredentials(login, password)).Wait();
        }

        protected void SetStreamMetadata(string streamId, long expectedVersion, StreamMetadata metadata, UserCredentials credentials)
        {
            Connection.SetStreamMetadataAsync(AdjustStreamId(streamId), expectedVersion, metadata, credentials).Wait();
        }

        protected void Expect<T>(Action action) where T : Exception
        {
            Assert.That(() => action(), Throws.Exception.InstanceOf<AggregateException>().With.InnerException.InstanceOf<T>());
        }

        protected void ExpectNoException(Action action)
        {
            Assert.That(() => action(), Throws.Nothing);
        }

        protected EventData[] CreateEvents()
        {
            return new[] { new EventData(Guid.NewGuid(), "some-type", false, new byte[] { 1, 2, 3 }, null) };
        }
    }
}