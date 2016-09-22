using System;
using System.Collections.Generic;
using System.Net;
using EventStore.ClientAPI.Internal;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Configuration;

namespace Eventstore.ClientAPI.Tests.Helpers
{
    public static class TestNode
    {
        private static readonly Endpoints Configuration = new Endpoints(ReadConfig());
        public static IPEndPoint TcpSecEndPoint => Configuration.TcpSecEndPoint;

        public static IPEndPoint TcpEndPoint => Configuration.TcpEndPoint;
        public static IPEndPoint HttpEndPoint => Configuration.HttpEndPoint;
        public static IPEndPoint GossipEndPoint => Configuration.GossipEndPoint;

        public static UserCredentials AdminCredentials => Configuration.AdminCredentials;
        public static IPEndPoint BlackHole => Configuration.BlackHole;

        class Endpoints
        {
            public Endpoints(IConfigurationRoot configuration)
            {
                TcpEndPoint = Parse(configuration["TCP"]);
                TcpSecEndPoint = Parse(configuration["SSLTCP"]);
                BlackHole = Parse(configuration["BLACKHOLE"]);
                HttpEndPoint = Parse(configuration["HTTP"]);
                GossipEndPoint = Parse(configuration["GOSSIP"]);
                var creds = configuration["ADMINCREDENTIALS"].Split(':');
                AdminCredentials = new UserCredentials(creds[0], creds[1]);
            }

            IPEndPoint Parse(string connection)
            {
                var uri = new Uri(connection);
                return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
            }

            public IPEndPoint TcpSecEndPoint { get; }

            public IPEndPoint TcpEndPoint { get; }
            public IPEndPoint HttpEndPoint { get; }
            public IPEndPoint GossipEndPoint { get; }

            public UserCredentials AdminCredentials { get; }
            public IPEndPoint BlackHole { get; }
        }

        static IConfigurationRoot ReadConfig()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(Defaults())
                .AddEnvironmentVariables("EVENTSTORE_NETCORE_CLIENT_TEST:")
                .Build();
        }

        private static IEnumerable<KeyValuePair<string, string>> Defaults()
        {
            yield return
                new KeyValuePair<string, string>("TCP", new IPEndPoint(IPAddress.Loopback, 1113).ToESTcpUri().ToString())
                ;
            yield return
                new KeyValuePair<string, string>("SSLTCP",
                    new IPEndPoint(IPAddress.Loopback, 1114).ToESTcpUri().ToString());
            yield return
                new KeyValuePair<string, string>("BLACKHOLE",
                    new IPEndPoint(IPAddress.Loopback, 25).ToESTcpUri().ToString());
            yield return
                new KeyValuePair<string, string>("HTTP",
                    new IPEndPoint(IPAddress.Loopback, 2113).ToESTcpUri().ToString());
            yield return
                new KeyValuePair<string, string>("GOSSIP",
                    new IPEndPoint(IPAddress.Loopback, 2112).ToESTcpUri().ToString());
            yield return
                new KeyValuePair<string, string>("ADMINCREDENTIALS",
                    $"{DefaultData.AdminUsername}:{DefaultData.AdminPassword}");
        }
    }
}