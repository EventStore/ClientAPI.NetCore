using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eventstore.ClientAPI.Tests.Helpers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Common.Utils;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests.projectionsManager
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
    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_creating_one_time_projection : ProjectionSpecification
    {
        private string _streamName;
        private string _query;

        public override void Given()
        {
            _streamName = "test-stream-" + Guid.NewGuid().ToString();
            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");

            _query = CreateStandardQuery(_streamName);
        }

        public override void When()
        {
            _projManager.CreateOneTimeAsync(_query, _credentials).Wait();
        }

        [Test]
        public void should_create_projection()
        {
            var projections = _projManager.ListOneTimeAsync(_credentials).Result;
            Assert.AreEqual(1, projections.Count);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_creating_transient_projection : ProjectionSpecification
    {
        private string _streamName;
        private string _projectionName;
        private string _query;

        public override void Given()
        {
            _streamName = "test-stream-" + Guid.NewGuid().ToString();
            _projectionName = "when_creating_transient_projection";
            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");

            _query = CreateStandardQuery(_streamName);
        }

        public override void When()
        {
            _projManager.CreateTransientAsync(_projectionName, _query, _credentials).Wait();
        }

        [Test]
        public void should_create_projection()
        {
            var status = _projManager.GetStatusAsync(_projectionName, _credentials).Result;
            Assert.IsNotEmpty(status);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_creating_continuous_projection : ProjectionSpecification
    {
        private string _streamName;
        private string _emittedStreamName;
        private string _projectionName;
        private string _query;
        private string _projectionId;

        public override void Given()
        {
            _streamName = "test-stream-" + Guid.NewGuid();
            _projectionName = "when_creating_continuous_projection";
            _emittedStreamName = "emittedStream-" + Guid.NewGuid();
            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");

            _query = CreateEmittingQuery(_streamName, _emittedStreamName);
        }

        public override void When()
        {
            _projManager.CreateContinuousAsync(_projectionName, _query, _credentials).Wait();
        }

        [Test]
        public void should_create_projection()
        {
            var allProjections = _projManager.ListContinuousAsync(_credentials).Result;
            var proj = allProjections.FirstOrDefault(x => x.EffectiveName == _projectionName);
            _projectionId = proj.Name;
            Assert.IsNotNull(proj);
        }

        [Test]
        public void should_have_turn_on_emit_to_stream()
        {
            var events = _connection.ReadEventAsync(string.Format("$projections-{0}", _projectionId), 0, true, _credentials).Result;
            var data = Encoding.UTF8.GetString(events.Event.Value.Event.Data);
            var eventData = data.ParseJson<JObject>();
            Assert.IsTrue((bool)eventData["emitEnabled"]);
        }

    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_creating_continuous_projection_with_track_emitted_streams : ProjectionSpecification
    {
        private string _streamName;
        private string _emittedStreamName;
        private string _projectionName;
        private string _query;
        private string _projectionId;

        public override void Given()
        {
            _streamName = "test-stream-" + Guid.NewGuid();
            _projectionName = "when_creating_continuous_projection_with_track_emitted_streams";
            _emittedStreamName = "emittedStream-" + Guid.NewGuid();
            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");

            _query = CreateEmittingQuery(_streamName, _emittedStreamName);
        }

        public override void When()
        {
            _projManager.CreateContinuousAsync(_projectionName, _query, true, _credentials).Wait();
        }

        [Test]
        public void should_create_projection()
        {
            var allProjections = _projManager.ListContinuousAsync(_credentials).Result;
            var proj = allProjections.FirstOrDefault(x => x.EffectiveName == _projectionName);
            _projectionId = proj.Name;
            Assert.IsNotNull(proj);
        }

        [Test]
        public void should_enable_track_emitted_streams()
        { 
            var events = _connection.ReadEventAsync(string.Format("$projections-{0}", _projectionId), 0, true, _credentials).Result;
            var data = Encoding.UTF8.GetString(events.Event.Value.Event.Data);
            var eventData = data.ParseJson<JObject>();
            Assert.IsTrue((bool)eventData["trackEmittedStreams"]);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_disabling_projections : ProjectionSpecification
    {
        private string _streamName;
        private string _projectionName;
        private string _query;

        public override void Given()
        {
            _streamName = "test-stream-" + Guid.NewGuid().ToString();
            _projectionName = "when_disabling_projection";
            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");

            _query = CreateStandardQuery(_streamName);

            _projManager.CreateContinuousAsync(_projectionName, _query, _credentials).Wait();
        }

        public override void When()
        {
            _projManager.DisableAsync(_projectionName, _credentials).Wait();
        }

        [Test]
        public void should_stop_the_projection()
        {
            var projectionStatus = _projManager.GetStatusAsync(_projectionName, _credentials).Result;
            var status = projectionStatus.ParseJson<JObject>()["status"].ToString();
            Assert.IsTrue(status.Contains("Stopped"), "Status did not contain 'Stopped' : {0}", status);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_enabling_projections : ProjectionSpecification
    {
        private string _streamName;
        private string _projectionName;
        private string _query;

        public override void Given()
        {
            _streamName = "test-stream-" + Guid.NewGuid().ToString();
            _projectionName = "when_enabling_projections";
            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");

            _query = CreateStandardQuery(_streamName);

            _projManager.CreateContinuousAsync(_projectionName, _query, _credentials).Wait();
            _projManager.DisableAsync(_projectionName, _credentials).Wait();
        }

        public override void When()
        {
            _projManager.EnableAsync(_projectionName, _credentials).Wait();
        }

        [Test]
        public void should_reenable_projection()
        {
            var projectionStatus = _projManager.GetStatusAsync(_projectionName, _credentials).Result;
            var status = projectionStatus.ParseJson<JObject>()["status"].ToString();
            Assert.IsTrue(status.Contains("Running"), "Status did not contain 'Running' : {0}", status);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_listing_all_projections : ProjectionSpecification
    {
        private List<ProjectionDetails> _result;
        public override void Given()
        {
        }

        public override void When()
        {
            _result = _projManager.ListAllAsync(_credentials).Result.ToList();
        }

        [Test]
        public void should_return_all_projections()
        {
            Assert.IsNotEmpty(_result);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_listing_one_time_projections : ProjectionSpecification
    {
        private List<ProjectionDetails> _result;
        public override void Given()
        {
            CreateOneTimeProjection();
        }

        public override void When()
        {
            _result = _projManager.ListOneTimeAsync(_credentials).Result.ToList();
        }

        [Test]
        public void should_return_projections()
        {
            Assert.IsNotEmpty(_result);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_listing_continuous_projections : ProjectionSpecification
    {
        private List<ProjectionDetails> _result;
        private string _projectionName;

        public override void Given()
        {
            _projectionName = Guid.NewGuid().ToString();
            CreateContinuousProjection(_projectionName);
        }

        public override void When()
        {
            _result = _projManager.ListContinuousAsync(_credentials).Result.ToList();
        }

        [Test]
        public void should_return_continuous_projections()
        {
            Assert.IsTrue(_result.Any(x => x.EffectiveName == _projectionName));
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_a_projection_is_running : ProjectionSpecification
    {
        private string _projectionName;
        private string _streamName;
        private string _query;

        public override void Given()
        {
            _projectionName = "when_getting_projection_information";
            _streamName = "test-stream-" + Guid.NewGuid().ToString();

            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");
        }

        public override void When()
        {
            _query = CreateStandardQuery(_streamName);
            _projManager.CreateContinuousAsync(_projectionName, _query, _credentials).Wait();
        }

        [Test]
        public void should_be_able_to_get_the_projection_state()
        {
            var state = _projManager.GetStateAsync(_projectionName, _credentials).Result;
            Assert.IsNotEmpty(state);
        }

        [Test]
        public void should_be_able_to_get_the_projection_status()
        {
            var status = _projManager.GetStatusAsync(_projectionName, _credentials).Result;
            Assert.IsNotEmpty(status);
        }

        [Test]
        public void should_be_able_to_get_the_projection_result()
        {
            var result = _projManager.GetResultAsync(_projectionName, _credentials).Result;
            Assert.AreEqual("{\"count\":1}", result);
        }

        [Test]
        public void should_be_able_to_get_the_projection_query()
        {
            var query = _projManager.GetQueryAsync(_projectionName, _credentials).Result;
            Assert.AreEqual(_query, query);
        }
    }

    [TestFixture]
    [Category("ProjectionsManager")]
    public class when_updating_a_projection_query : ProjectionSpecification
    {
        private string _projectionName;
        private string _streamName;
        private string _newQuery;

        public override void Given()
        {
            _projectionName = "when_updating_a_projection_query";
            _streamName = "test-stream-" + Guid.NewGuid().ToString();

            PostEvent(_streamName, "testEvent", "{\"A\":\"1\"}");
            PostEvent(_streamName, "testEvent", "{\"A\":\"2\"}");

            var origQuery = CreateStandardQuery(_streamName);
            _newQuery = CreateStandardQuery("DifferentStream");
            _projManager.CreateContinuousAsync(_projectionName, origQuery, _credentials).Wait();
        }

        public override void When()
        {
            _projManager.UpdateQueryAsync(_projectionName, _newQuery, _credentials).Wait();
        }

        [Test]
        public void should_update_the_projection_query()
        {
            var query = _projManager.GetQueryAsync(_projectionName, _credentials).Result;
            Assert.AreEqual(_newQuery, query);
        }
    }
}
