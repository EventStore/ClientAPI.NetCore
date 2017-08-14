using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI.Projections;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
            _result = Enumerable.ToList<ProjectionDetails>(_projManager.ListContinuousAsync(_credentials).Result);
        }

        [Test]
        public void should_return_continuous_projections()
        {
            Assert.IsTrue(Enumerable.Any<ProjectionDetails>(_result, x => x.EffectiveName == _projectionName));
        }
    }
}