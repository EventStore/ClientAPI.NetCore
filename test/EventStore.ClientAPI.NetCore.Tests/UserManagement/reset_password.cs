using System;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.Transport.Http;
using NUnit.Framework;

namespace Eventstore.ClientAPI.Tests.UserManagement
{
    public class reset_password : TestWithUser
    {
        [Test]
        public void null_user_name_throws()
        {
            Assert.Throws<ArgumentNullException>(() =>_manager.ResetPasswordAsync(null, "foo", new UserCredentials("admin", "changeit")).Wait());
        }

        [Test]
        public void empty_user_name_throws()
        {
            Assert.Throws<ArgumentNullException>(() =>_manager.ResetPasswordAsync("", "foo", new UserCredentials("admin", "changeit")).Wait());
        }

        [Test]
        public void empty_password_throws()
        {
            Assert.Throws<ArgumentNullException>(() =>_manager.ResetPasswordAsync(_username, "", new UserCredentials("admin", "changeit")).Wait());
        }

        [Test]
        public void null_password_throws()
        {
            Assert.Throws<ArgumentNullException>(() =>_manager.ResetPasswordAsync(_username, null, new UserCredentials("admin", "changeit")).Wait());
        }

        [Test]
        public void can_reset_password()
        {
            _manager.ResetPasswordAsync(_username, "foo", new UserCredentials("admin", "changeit")).Wait();
            var ex = Assert.Throws<AggregateException>(
                () => _manager.ChangePasswordAsync(_username, "password", "foobar", new UserCredentials(_username, "password")).Wait()
            );
            Assert.AreEqual(HttpStatusCode.Unauthorized, ((UserCommandFailedException)ex.InnerException).HttpStatusCode);
        }
    }
}