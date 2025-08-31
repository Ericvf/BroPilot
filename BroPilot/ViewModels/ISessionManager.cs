using System.Collections.Generic;
using System.Threading.Tasks;

namespace BroPilot.ViewModels
{
    public interface ISessionManager
    {
        Task UpdateSession(Session session);

        Task DeleteSession(Session session);

        IEnumerable<Session> LoadSessions();
    }
}
