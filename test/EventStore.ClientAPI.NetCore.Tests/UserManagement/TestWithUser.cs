using System;
using EventStore.ClientAPI.SystemData;

namespace EventStore.Core.Tests.ClientAPI.UserManagement
{
    public class TestWithUser : TestWithNode
    {
        protected string _username = Guid.NewGuid().ToString();
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            _manager.CreateUserAsync(_username, "name", new[] {"foo", "admins"}, "password", new UserCredentials("admin", "changeit")).Wait();
        }
    }
}