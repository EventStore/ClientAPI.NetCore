using System;
using EventStore.ClientAPI.Common.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
}