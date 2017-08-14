using System;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using EventStore.Core.Tests.ClientAPI.Helpers;

namespace EventStore.Core.Tests.ClientAPI
{
    public abstract class ProjectionSpecification
    {
        protected readonly ProjectionsManager _projManager;
        protected readonly IEventStoreConnection _connection;
        protected readonly UserCredentials _credentials;
        protected ProjectionSpecification()
        {
            _credentials = TestNode.AdminCredentials;
            _connection = TestConnection.Create(TcpType.Normal, _credentials);
            _connection.ConnectAsync().Wait();
            _projManager = new ProjectionsManager(new ConsoleLogger(), TestNode.HttpEndPoint, TimeSpan.FromSeconds(20));
            Given();
            When();
        }

        public virtual void Given() { }
        public virtual void When() { }

        protected EventData CreateEvent(string eventType, string data)
        {
            return new EventData(Guid.NewGuid(), eventType, true, Encoding.UTF8.GetBytes(data), null);
        }

        protected void PostEvent(string stream, string eventType, string data)
        {
            _connection.AppendToStreamAsync(stream, ExpectedVersion.Any, new[] { CreateEvent(eventType, data) }).Wait();
        }

        protected void CreateOneTimeProjection()
        {
            var query = CreateStandardQuery(Guid.NewGuid().ToString());
            _projManager.CreateOneTimeAsync(query, _credentials).Wait();
        }

        protected void CreateContinuousProjection(string projectionName)
        {
            var query = CreateStandardQuery(Guid.NewGuid().ToString());
            _projManager.CreateContinuousAsync(projectionName, query, _credentials).Wait();
        }

        protected string CreateStandardQuery(string stream)
        {
            return @"fromStream(""" + stream + @""")
                .when({
                     ""$any"":function(s,e) {
                         s.count = 1;
                         return s;
                     }
             });";
        }

        protected string CreateEmittingQuery(string stream, string emittingStream)
        {
            return @"fromStream(""" + stream + @""")
                 .when({
                     ""$any"":function(s,e) {
                         emit(""" + emittingStream + @""", ""emittedEvent"", e);
                     } 
                 });";
        }
    }
}
