using System;
using System.Linq;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    public class when_connecting_with_connection_string
    {
        [Test]
        [Category("Network")]
        public void should_not_throw_when_connect_to_is_set()
        {
            string connectionString = string.Format("ConnectTo=tcp://{0};", TestNode.TcpEndPoint);
            using(var connection = EventStoreConnection.Create(connectionString))
            {
                Assert.DoesNotThrow(connection.ConnectAsync().Wait);
                connection.Close();
            }
        }

        [Test]
        public void should_not_throw_when_only_gossip_seeds_is_set()
        {
            string connectionString = string.Format("GossipSeeds={0};", TestNode.GossipEndPoint);
            IEventStoreConnection connection = null;

            Assert.DoesNotThrow(() => connection = EventStoreConnection.Create(connectionString));
            Assert.AreEqual(TestNode.GossipEndPoint, connection.Settings.GossipSeeds.First().EndPoint);

            connection.Dispose();
        }

        [Test]
        public void should_throw_when_gossip_seeds_and_connect_to_is_set()
        {
            string connectionString = string.Format("ConnectTo=tcp://{0};GossipSeeds={1}", TestNode.TcpEndPoint, TestNode.GossipEndPoint);
            Assert.Throws<NotSupportedException>(() => EventStoreConnection.Create(connectionString));
        }

        [Test]
        public void should_throw_when_neither_gossip_seeds_nor_connect_to_is_set()
        {
            string connectionString = string.Format("HeartBeatTimeout=2000");
            Assert.Throws<Exception>(() => EventStoreConnection.Create(connectionString));
        }
    }
}