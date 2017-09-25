using System;
using System.Linq;
using InSearch.Core.Data;
using InSearch.Core.Domain.Users;
using InSearch.Core;

namespace InSearch.Services.Users
{
    public class SessionService : ISessionService
    {
        #region Fields
        private readonly IRepository<Session> sessionRepository;
        private readonly IWorkContext workContext;
        private readonly IWebHelper webHelper;
        #endregion Fields

        #region Constructors
        public SessionService(
            IRepository<Session> sessionRepository,
            IWorkContext workContext,
            IWebHelper webHelper)
        {
            this.sessionRepository = sessionRepository;
            this.workContext = workContext;
            this.webHelper = webHelper;
        }
        #endregion Constructors

        #region Methods
        public int GetCountSession()
        {
            var query = sessionRepository.Table;
            query = query.Where(s => s.ExpireOnUtc == null);

            return query.Count();
        }

        public Session GetSessionById(int id)
        {
            if (id == 0)
                return new Session();

            return sessionRepository.GetById(id);
        }
        public Session GetSessionByUser(User user)
        {
            if (user == null)
                return new Session();

            var query = sessionRepository.Table;
            query = query.Where(s => s.UserId == user.Id && s.ExpireOnUtc == null);
            query = query.OrderByDescending(s => s.CreatedOnUtc);

            return query.FirstOrDefault();
        }
        public Session GetSessionByIP(string ip)
        {
            if (!ip.HasValue())
                return new Session();

            var query = sessionRepository.Table;
            query = query.Where(s => s.IPAddress == ip && s.ExpireOnUtc == null);
            query = query.OrderByDescending(s => s.CreatedOnUtc);

            return query.FirstOrDefault();
        }

        public Session AddNewSession()
        {
            var session = new Session()
            {
                UserId = workContext.CurrentUser.Id,
                IPAddress = webHelper.GetCurrentIpAddress()
            };

            CheckAndCloseOldSession(session);

            sessionRepository.Insert(session);

            return session;
        }
        public Session AddNewSession(User user, string ip)
        {
            var session = new Session()
            {
                UserId = user.Id,
                IPAddress = ip
            };
            sessionRepository.Insert(session);

            return session;
        }
        public void CloseSession(User user)
        {
            var session = GetSessionByUser(user);
            if (session != null)
            {
                session.ExpireOnUtc = DateTime.UtcNow;
                sessionRepository.Update(session);
            }
        }

        public bool CheckAndCloseOldSession(Session session, bool closeSession = true)
        {
            var query = sessionRepository.Table.Where(q => q.ExpireOnUtc == null);
            if (session.UserId != 0)
                query = query.Where(q => q.UserId == session.UserId);
            if (session.IPAddress.HasValue())
                query = query.Where(q => q.IPAddress == session.IPAddress);
            if (closeSession)
            {
                var sessions = query.ToList();
                var nowDate = DateTime.UtcNow;
                sessions.ForEach(s => {
                    s.ExpireOnUtc = nowDate;
                    sessionRepository.Update(s);
                });
            }
            return query.Any();
        }
        #endregion Methods
    }
}
