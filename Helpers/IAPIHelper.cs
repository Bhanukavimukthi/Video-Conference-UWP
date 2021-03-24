using Eminutes.UWP.Model;

namespace Eminutes.UWP.Helpers
{
    public interface IAPIHelper
    {
        System.Threading.Tasks.Task<AuthenticatedUser> Authenticate(string username, string password);
        System.Threading.Tasks.Task<System.Collections.Generic.List<Rootobject>> GetMeetings();
    }
}