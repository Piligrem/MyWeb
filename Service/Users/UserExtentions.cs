using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using InSearch.Core.Domain.Users;
using InSearch.Core.Infrastructure;
//using InSearch.Services.Common;
//using InSearch.Services.Localization;

namespace InSearch
{
    public static class UserExtentions
    {
        /// <summary>
        /// Gets a value indicating whether user is in a certain user role
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="userRoleSystemName">User role system name</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active user roles</param>
        /// <returns>Result</returns>
        public static bool IsInUserRole(this User user,
            string userRoleSystemName, bool onlyActiveUserRoles = true)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (String.IsNullOrEmpty(userRoleSystemName))
                throw new ArgumentNullException("userRoleSystemName");

            //var tempFull = user.Roles.Where(cr => !onlyActiveUserRoles).Where(cr => cr.Name == userRoleSystemName).FirstOrDefault();
            //var tempFirst = user.Roles.Where(cr => !onlyActiveUserRoles).FirstOrDefault();
            //var tempSecond = user.Roles.Where(cr => cr.Name == userRoleSystemName).FirstOrDefault() != null;

            var result = user.UserRoles
                //.Where(cr => !onlyActiveUserRoles)
                .Where(cr => cr.Name == userRoleSystemName)
                .FirstOrDefault() != null;
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the user is a built-in record for background tasks
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Result</returns>
        public static bool IsBackgroundTaskAccount(this User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.IsSystemAccount)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether user a search engine
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Result</returns>
        public static bool IsSearchEngineAccount(this User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.IsSystemAccount)
                return false;
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether user is administrator
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active user roles</param>
        /// <returns>Result</returns>
        public static bool IsAdmin(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, "Administrators", onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether user is super administrator
        /// </summary>
        /// <remarks>codehint: sm-add</remarks>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active user roles</param>
        /// <returns>Result</returns>
        public static bool IsSuperAdmin(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, "SuperAdministrators", onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether user is a forum moderator
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active user roles</param>
        /// <returns>Result</returns>
        public static bool IsForumModerator(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, "ForumModerators", onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether user is registered
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active user roles</param>
        /// <returns>Result</returns>
        public static bool IsRegistered(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, "Registered", onlyActiveUserRoles);
        }

        /// <summary>
        /// Gets a value indicating whether user is guest
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles">A value indicating whether we should look only in active user roles</param>
        /// <returns>Result</returns>
        public static bool IsGuest(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, "Guests", onlyActiveUserRoles);
        }

        public static string GetFullName(this User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            var firstName = user.FirstName;
            var lastName = user.LastName;

            string fullName = "";
            if (!String.IsNullOrWhiteSpace(firstName) && !String.IsNullOrWhiteSpace(lastName))
            {
                fullName = string.Format("{0} {1}", firstName, lastName);
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(firstName))
                    fullName = firstName;

                if (!String.IsNullOrWhiteSpace(lastName))
                    fullName = lastName;
            }
            return fullName;
        }

        /// <summary>
        /// Formats the user name
        /// </summary>
        /// <param name="user">Source</param>
        /// <returns>Formatted text</returns>
        public static string FormatUserName(this User user)
        {
            return FormatUserName(user, false);
        }

        /// <summary>
        /// Formats the user name
        /// </summary>
        /// <param name="user">Source</param>
        /// <param name="stripTooLong">Strip too long user name</param>
        /// <returns>Formatted text</returns>
        public static string FormatUserName(this User user, bool stripTooLong)
        {
            //if (user == null)
            //    return string.Empty;

            //if (user.IsGuest())
            //{
            //    return EngineContext.Current.Resolve<ILocalizationService>().GetResource("User.Guest");
            //}

            string result = string.Empty;
            //switch (EngineContext.Current.Resolve<UserSettings>().UserNameFormat)
            switch ((new UserSettings()).UserNameFormat)
            {
                case UserNameFormat.ShowEmails:
                    result = user.Email;
                    break;
                case UserNameFormat.ShowFullNames:
                    result = user.GetFullName();
                    break;
                case UserNameFormat.ShowUsernames:
                    result = user.Username;
                    break;
                default:
                    break;
            }

            //if (stripTooLong)
            //{
            //    int maxLength = 0; // TODO make this setting configurable
            //    if (maxLength > 0 && result.Length > maxLength)
            //    {
            //        result = result.Substring(0, maxLength);
            //    }
            //}
            //
            return result;
        }

        #region Commented for a future
        #region Shopping cart
        //public static int CountProductsInCart(this User user, ShoppingCartType cartType)
        //{
        //    int count = user.ShoppingCartItems
        //        .Filter(cartType)
        //        .Where(x => x.ParentItemId == null)
        //        .Sum(x => x.Quantity);

        //    return count;
        //}
        //public static List<OrganizedShoppingCartItem> GetCartItems(this User user, ShoppingCartType cartType, bool orderById = false)
        //{
        //    var rawItems = user.ShoppingCartItems.Filter(cartType);

        //    if (orderById)
        //        rawItems = rawItems.OrderByDescending(x => x.Id);

        //    var organizedItems = rawItems.ToList().Organize();

        //    return organizedItems.ToList();
        //}
        #endregion Shopping cart
        #endregion Commented for a future
    }
}
