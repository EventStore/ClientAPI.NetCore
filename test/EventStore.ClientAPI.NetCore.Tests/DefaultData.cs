using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI.SystemData;

namespace Eventstore.ClientAPI.Tests
{
    public class DefaultData
    {
        public static string AdminUsername = SystemUsers.Admin;
        public static string AdminPassword = SystemUsers.DefaultAdminPassword;
        public static UserCredentials AdminCredentials = new UserCredentials(AdminUsername, AdminPassword);
        public static NetworkCredential AdminNetworkCredentials = new NetworkCredential(AdminUsername, AdminPassword);
    }
}
