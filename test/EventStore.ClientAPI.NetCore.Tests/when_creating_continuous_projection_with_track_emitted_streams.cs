using System;
using System.Linq;
using System.Text;
using EventStore.ClientAPI.Common.Utils;
using EventStore.ClientAPI.Projections;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
            var proj = Enumerable.FirstOrDefault<ProjectionDetails>(allProjections, x => x.EffectiveName == _projectionName);
            _projectionId = proj.Name;
            Assert.IsNotNull(proj);
        }

        [Test]
        public void should_enable_track_emitted_streams()
        { 
            var events = _connection.ReadEventAsync(string.Format((string) "$projections-{0}", (object) _projectionId), 0, true, _credentials).Result;
            var data = Encoding.UTF8.GetString(events.Event.Value.Event.Data);
            var eventData = data.ParseJson<JObject>();
            Assert.IsTrue((bool)eventData["trackEmittedStreams"]);
        }
    }
}