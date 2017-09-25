using Microsoft.AspNet.SignalR.Hubs;
using InSearch.Core.Domain.Users;
using InSearch.Core.Fakes;
using InSearch.Services.Authentication;
using InSearch.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InSearch.Services.Common;

namespace InSearch.Services.Extensions
{
    public static class HubContextExtensions
    {
        private static readonly string UserCookieName = "insearch.user";

        public static User CurrentUser(this HubCallerContext context)
        {
            var authenticationService = EngineContext.Current.Resolve<IAuthenticationService>();
            var userService = EngineContext.Current.Resolve<IUserService>();

            User user = null;
            if (context == null)
                return null;

            //registered user
            if (user == null || user.Deleted || !user.Active)
            {
                user = authenticationService.GetAuthenticatedUser();
            }

            //impersonate user if required (currently used for 'phone order' support)
            if (user != null && !user.Deleted && user.Active)
            {
                int? impersonatedUserId = user.GetAttribute<int?>(SystemUserAttributeNames.ImpersonatedUserId);
                if (impersonatedUserId.HasValue && impersonatedUserId.Value > 0)
                {
                    var impersonatedUser = userService.GetUserById(impersonatedUserId.Value);
                    if (impersonatedUser != null && !impersonatedUser.Deleted && impersonatedUser.Active)
                    {
                        user = impersonatedUser;
                    }
                }
            }

            //load guest user
            if (user == null || user.Deleted || !user.Active)
            {
                var userCookie = context.Request.Cookies[UserCookieName];
                if (userCookie != null && !String.IsNullOrEmpty(userCookie.Value))
                {
                    Guid userGuid;
                    if (Guid.TryParse(userCookie.Value, out userGuid))
                    {
                        var userByCookie = userService.GetUserByGuid(userGuid);
                        if (userByCookie != null &&
                            //this user (from cookie) should not be registered
                            !userByCookie.IsRegistered() &&
                            //it should not be a built-in 'search engine' user account
                            !userByCookie.IsSearchEngineAccount())
                            user = userByCookie;
                    }
                }
            }

            return user;
        }
    }
}
