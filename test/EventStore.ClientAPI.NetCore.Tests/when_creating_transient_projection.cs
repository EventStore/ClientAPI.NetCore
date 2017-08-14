using System;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
            Assert.IsNotEmpty((string) status);
        }
    }
}