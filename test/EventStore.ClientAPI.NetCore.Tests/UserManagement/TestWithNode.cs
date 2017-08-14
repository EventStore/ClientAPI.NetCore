using System;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.UserManagement;
using EventStore.Core.Tests.ClientAPI.Helpers;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI.UserManagement
{
    [Category("LongRunning")]
    public class TestWithNode 
    {
        protected UsersManager _manager;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            _manager = new UsersManager(new NoopLogger(), TestNode.HttpEndPoint, TimeSpan.FromSeconds(5));
        }

        protected virtual IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

    }
}