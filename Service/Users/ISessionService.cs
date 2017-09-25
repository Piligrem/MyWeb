using System;
using InSearch.Core.Domain.Users;

namespace InSearch.Services.Users
{
    public interface ISessionService
    {
        Session AddNewSession();
        Session AddNewSession(User user, string ip);
        int GetCountSession();
        void CloseSession(User user);
        bool CheckAndCloseOldSession(Session session, bool closeSession = true);
        Session GetSessionById(int id);
        Session GetSessionByIP(string ip);
        Session GetSessionByUser(InSearch.Core.Domain.Users.User user);
    }
}
