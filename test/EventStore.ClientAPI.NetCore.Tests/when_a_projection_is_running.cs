using System;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
            Assert.IsNotEmpty((string) state);
        }

        [Test]
        public void should_be_able_to_get_the_projection_status()
        {
            var status = _projManager.GetStatusAsync(_projectionName, _credentials).Result;
            Assert.IsNotEmpty((string) status);
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
}