using EventStore.ClientAPI.SystemData;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests.UserManagement
{
    public class get_current_user : TestWithNode
    {
        [Test]
        public void returns_the_current_user()
        {
            var x = _manager.GetCurrentUserAsync(new UserCredentials("admin", "changeit")).Result;
            Assert.AreEqual("admin", x.LoginName);
            Assert.AreEqual("Event Store Administrator", x.FullName);
        }
    }
}