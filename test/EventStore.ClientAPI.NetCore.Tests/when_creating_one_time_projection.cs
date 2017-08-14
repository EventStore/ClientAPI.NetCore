using System;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
}