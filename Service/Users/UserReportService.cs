using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InSearch.Core.Data;
using InSearch.Core.Domain.Users;
using InSearch.Services.Helpers;

namespace InSearch.Services.Users
{
    public partial class UserReportService : IUserReportService
    {
        #region Fields

        private readonly IRepository<User> _userRepository;
        private readonly IUserService _userService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<Session> _sessionRepository;
        #endregion

        #region Ctor
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userRepository">User repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="userService">User service</param>
        /// <param name="dateTimeHelper">Date time helper</param>
        public UserReportService(IRepository<User> userRepository,
            IUserService userService, IRepository<Session> sessionRepository,
            IDateTimeHelper dateTimeHelper)
        {
            this._userRepository = userRepository;
            this._userService = userService;
            this._sessionRepository = sessionRepository;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets a report of users registered in the last days
        /// </summary>
        /// <param name="days">Users registered in the last days</param>
        /// <returns>Number of registered users</returns>
        public virtual int GetRegisteredUsersReport(int days)
        {
            DateTime date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).Value.AddDays(-days);

            var registeredUserRole = _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered);
            if (registeredUserRole == null)
                return 0;

            var query = from c in _userRepository.Table
                        from cr in c.UserRoles
                        where !c.Deleted &&
                        cr.Id == registeredUserRole.Id &&
                        c.CreatedOnUtc >= date
                        //&& c.CreatedOnUtc <= DateTime.UtcNow
                        select c;
            int count = query.Count();
            return count;
        }

        public virtual int GetCountOnlineUsers()
        {
            var query = _sessionRepository.Table;

            query = query.Where(s => s.ExpireOnUtc != null);

            return query.Count();
        }
        #endregion
    }
}
