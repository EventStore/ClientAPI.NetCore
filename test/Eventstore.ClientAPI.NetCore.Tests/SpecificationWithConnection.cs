using System.Net;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    public abstract class SpecificationWithConnection
    {
        protected IEventStoreConnection _conn;
        
        protected virtual void Given()
        {
        }

        protected abstract void When();

        protected IEventStoreConnection BuildConnection()
        {
            return TestConnection.Create(TcpType.Normal);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _conn = BuildConnection();
            _conn.ConnectAsync().Wait();
            Given();
            When();
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            _conn.Close();
        }
    }
}
