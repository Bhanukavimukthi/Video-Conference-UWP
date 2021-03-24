using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Eminutes.UWP.Model
{
    class Model
    {
    }
    public class UserInRoom
    {
        public string RoomId { get; set; }
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public string orgPermission { get; set; }
        public string Stream { get; set; }
        public string Email { get; set; }

    }

    public class remoteVideo
    {
        public MediaElement remote { get; set; }
        public string id { get; set; }
    }

}
