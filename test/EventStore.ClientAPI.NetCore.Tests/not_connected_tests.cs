using System;
using System.Net;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Internal;
using EventStore.Core.Tests.ClientAPI.Helpers;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class not_connected_tests
    {
        private readonly TcpType _tcpType = TcpType.Normal;


        [Test]
        public void should_timeout_connection_after_configured_amount_time_on_conenct()
        {
            var closed = new ManualResetEventSlim();
            var settings =
                ConnectionSettings.Create()
                    .EnableVerboseLogging()
                    .LimitReconnectionsTo(0)
                    .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(0))
                    .FailOnNoServerResponse()
                    .WithConnectionTimeoutOf(TimeSpan.FromMilliseconds(1000));

            if (_tcpType == TcpType.Ssl)
                settings.UseSslConnection("ES", false);

            var ip = new IPAddress(new byte[] { 8, 8, 8, 8 }); //NOTE: This relies on Google DNS server being configured to swallow nonsense traffic
            const int port = 4567;
            using (var connection = EventStoreConnection.Create(settings, new IPEndPoint(ip, port).ToESTcpUri()))
            {
                connection.Closed += (s, e) => closed.Set();
                connection.Connected += (s, e) => Console.WriteLine("EventStoreConnection '{0}': connected to [{1}]...", e.Connection.ConnectionName, e.RemoteEndPoint);
                connection.Reconnecting += (s, e) => Console.WriteLine("EventStoreConnection '{0}': reconnecting...", e.Connection.ConnectionName);
                connection.Disconnected += (s, e) => Console.WriteLine("EventStoreConnection '{0}': disconnected from [{1}]...", e.Connection.ConnectionName, e.RemoteEndPoint);
                connection.ErrorOccurred += (s, e) => Console.WriteLine("EventStoreConnection '{0}': error = {1}", e.Connection.ConnectionName, e.Exception);
                connection.ConnectAsync().Wait();

                if (!closed.Wait(TimeSpan.FromSeconds(15)))
                    Assert.Fail("Connection timeout took too long.");
            }

        }

    }
}