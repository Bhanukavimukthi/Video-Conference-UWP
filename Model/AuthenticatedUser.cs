using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eminutes.UWP.Model
{
    public class AuthenticatedUser
    {
        public string jwtToken { get; set; }
        public string UserName { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string permissionLevel { get; set; }
        public int appUserId { get; set; }

    }
}
