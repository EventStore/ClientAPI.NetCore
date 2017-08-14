using System;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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