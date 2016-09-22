using System;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.UserManagement;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests.UserManagement
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