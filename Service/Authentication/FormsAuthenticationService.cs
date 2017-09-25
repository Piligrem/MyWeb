using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using InSearch.Core.Domain.Users;
using InSearch.Services.Users;
using Newtonsoft.Json;
using InSearch.Core.Domain.Security;

namespace InSearch.Services.Authentication
{
    public partial class FormsAuthenticationService : IAuthenticationService
    {
        private readonly HttpContextBase _httpContext;
        private readonly IUserService _userService;
        private readonly UserSettings _userSettings;
        private readonly TimeSpan _expirationTimeSpan;

        private User _cachedUser;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="userService">User service</param>
        /// <param name="userSettings">User settings</param>
        public FormsAuthenticationService(HttpContextBase httpContext,
            IUserService userService, UserSettings userSettings)
        {
            this._httpContext = httpContext;
            this._userService = userService;
            this._userSettings = userSettings;
            this._expirationTimeSpan = FormsAuthentication.Timeout;
        }


        public virtual void SignIn(User user, bool createPersistentCookie)
        {
            var now = DateTime.UtcNow.ToLocalTime();
            #region ??? Возможно удалить ???
            var ticket = new FormsAuthenticationTicket(
                1 /*version*/,
                _userSettings.UsernamesEnabled ? user.Username : user.Email,
                now,
                now.Add(_expirationTimeSpan),
                createPersistentCookie,
                _userSettings.UsernamesEnabled ? user.Username : user.Email,
                FormsAuthentication.FormsCookiePath);
            #endregion ??? Возможно удалить ???

            #region For Identity
            //var roles = user.Roles.Select(m => m.Name).ToArray();

            //var serializeModel = new CustomPrincipalSerializeModel();
            //serializeModel.Id = user.Id;
            //serializeModel.FirstName = user.FirstName;
            //serializeModel.LastName = user.LastName;
            //serializeModel.Roles = roles;

            //string userData = JsonConvert.SerializeObject(serializeModel);
            //var ticket = new FormsAuthenticationTicket(
            //    1,
            //    user.Email,
            //    DateTime.Now,
            //    DateTime.Now.AddMinutes(15),
            //    false,
            //    userData);
            #endregion For Identity

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            _httpContext.Response.Cookies.Add(cookie);
            _cachedUser = user;
        }

        public virtual void SignOut()
        {
            _cachedUser = null;
            FormsAuthentication.SignOut();
        }

        public virtual User GetAuthenticatedUser()
        {
            if (_cachedUser != null)
                return _cachedUser;

            if (_httpContext == null ||
                _httpContext.Request == null ||
                !_httpContext.Request.IsAuthenticated ||
                !(_httpContext.User.Identity is FormsIdentity))
            {
                return null;
            }

            var formsIdentity = (FormsIdentity)_httpContext.User.Identity;
            var user = GetAuthenticatedUserFromTicket(formsIdentity.Ticket);
            if (user != null && user.Active && !user.Deleted && user.IsRegistered())
                _cachedUser = user;
            return _cachedUser;
        }

        public virtual User GetAuthenticatedUserFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            var usernameOrEmail = ticket.UserData;

            if (String.IsNullOrWhiteSpace(usernameOrEmail))
                return null;
            var user = _userSettings.UsernamesEnabled
                ? _userService.GetUserByUsername(usernameOrEmail)
                : _userService.GetUserByEmail(usernameOrEmail);
            return user;
        }
    }
}
