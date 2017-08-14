using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI.Projections;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
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
            _result = Enumerable.ToList<ProjectionDetails>(_projManager.ListOneTimeAsync(_credentials).Result);
        }

        [Test]
        public void should_return_projections()
        {
            Assert.IsNotEmpty((IEnumerable) _result);
        }
    }
}