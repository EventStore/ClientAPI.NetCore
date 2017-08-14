using System.Net;
using EventStore.ClientAPI;
using EventStore.Core.Tests.ClientAPI.Helpers;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
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
