using System;
using System.Threading;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Internal;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests
{
    [TestFixture(TcpType.Normal), TestFixture(TcpType.Ssl), Category("LongRunning")]
    public class connect
    {
        private readonly TcpType _tcpType;

        public connect(TcpType tcpType)
        {
            _tcpType = tcpType;
        }

        //TODO GFY THESE NEED TO BE LOOKED AT IN LINUX
        [Test, Category("Network"), Category("WIN")]
        public void should_not_throw_exception_when_server_is_down()
        {
            using (var connection = TestConnection.Create(TestNode.BlackHole, _tcpType))
            {
                Assert.DoesNotThrow(() => connection.ConnectAsync().Wait());
            }
        }
        //TODO GFY THESE NEED TO BE LOOKED AT IN LINUX
        [Test, Category("Network"), Category("WIN")]
        public void should_throw_exception_when_trying_to_reopen_closed_connection()
        {
            var closed = new ManualResetEventSlim();
            var settings = ConnectionSettings.Create()
                                             .EnableVerboseLogging()
                                             .LimitReconnectionsTo(0)
                                             .WithConnectionTimeoutOf(TimeSpan.FromSeconds(10))
                                             .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(0))
                                             .FailOnNoServerResponse();
            if (_tcpType == TcpType.Ssl)
                settings.UseSslConnection("ES", false);

            using (var connection = EventStoreConnection.Create(settings, TestNode.BlackHole.ToESTcpUri()))
            {
                connection.Closed += (s, e) => closed.Set();

                connection.ConnectAsync().Wait();

                if (!closed.Wait(TimeSpan.FromSeconds(120))) // TCP connection timeout might be even 60 seconds
                    Assert.Fail("Connection timeout took too long.");

                Assert.That(() => connection.ConnectAsync().Wait(),
                            Throws.Exception.InstanceOf<AggregateException>()
                                  .With.InnerException.InstanceOf<InvalidOperationException>());
            }
        }

        //TODO GFY THIS TEST TIMES OUT IN LINUX.
        [Test, Category("Network"), Category("WIN")]
        public void should_close_connection_after_configured_amount_of_failed_reconnections()
        {
            var closed = new ManualResetEventSlim();
            var settings =
                ConnectionSettings.Create()
                                  .EnableVerboseLogging()
                                  .LimitReconnectionsTo(1)
                                  .WithConnectionTimeoutOf(TimeSpan.FromSeconds(10))
                                  .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(0))
                                  .FailOnNoServerResponse();
            if (_tcpType == TcpType.Ssl)
                settings.UseSslConnection("ES", false);
            
            using (var connection = EventStoreConnection.Create(settings, TestNode.BlackHole.ToESTcpUri()))
            {
                connection.Closed += (s, e) => closed.Set();
                connection.Connected += (s, e) => Console.WriteLine("EventStoreConnection '{0}': connected to [{1}]...", e.Connection.ConnectionName, e.RemoteEndPoint);
                connection.Reconnecting += (s, e) => Console.WriteLine("EventStoreConnection '{0}': reconnecting...", e.Connection.ConnectionName);
                connection.Disconnected += (s, e) => Console.WriteLine("EventStoreConnection '{0}': disconnected from [{1}]...", e.Connection.ConnectionName, e.RemoteEndPoint);
                connection.ErrorOccurred += (s, e) => Console.WriteLine("EventStoreConnection '{0}': error = {1}", e.Connection.ConnectionName, e.Exception);

                connection.ConnectAsync().Wait();

                if (!closed.Wait(TimeSpan.FromSeconds(120))) // TCP connection timeout might be even 60 seconds
                    Assert.Fail("Connection timeout took too long.");

                Assert.That(() => connection.AppendToStreamAsync("stream", ExpectedVersion.EmptyStream, TestEvent.NewTestEvent()).Wait(),
                            Throws.Exception.InstanceOf<AggregateException>()
                            .With.InnerException.InstanceOf<InvalidOperationException>());
            }
            
        }

    }
}
