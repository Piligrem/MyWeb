using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using InSearch.Core;
using InSearch.Core.Caching;
using InSearch.Core.Data;
using InSearch.Core.Domain.Common;
using InSearch.Core.Domain.Users;
using InSearch.Core.Events;
using InSearch.Services.Common;

namespace InSearch.Services.Users
{
    public partial class UserService : IUserService
    {
        #region Constants
        private const string USERROLES_ALL_KEY = "InSearch.userrole.all-{0}";
        private const string USERROLES_BY_SYSTEMNAME_KEY = "InSearch.userrole.systemname-{0}";
        private const string USERROLES_PATTERN_KEY = "InSearch.userrole.";
        private const string DeleteGuestUserCommand = "Delete From [User] From UserRole_Mapping URM Inner Join UserRole UR On URM.UserRole_Id = UR.Id Inner Join [User] U On URM.User_Id = U.Id Where U.IsSystemAccount=0 And UR.Id={0}";
        private const string DeleteAttributesGuestUserCommand = "Delete From [GenericAttribute] From UserRole_Mapping URM Inner Join UserRole UR On URM.UserRole_Id = UR.Id Inner Join [User] U On URM.User_Id = U.Id Inner Join GenericAttribute GA On GA.EntityId = U.Id Where GA.KeyGroup='{0}' And U.IsSystemAccount=0 And UR.Id={1}";
        #endregion Constants

        #region Fields
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDbContext _context;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="userRepository">User repository</param>
        /// <param name="userRoleRepository">User role repository</param>
        /// <param name="gaRepository">Generic attribute repository</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="eventPublisher">Event published</param>
        public UserService(ICacheManager cacheManager,
            IDbContext context,
            IRepository<User> userRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<Address> addressRepository,
            IRepository<GenericAttribute> gaRepository,
            IGenericAttributeService genericAttributeService,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._userRepository = userRepository;
            this._userRoleRepository = userRoleRepository;
            this._addressRepository = addressRepository;
            this._gaRepository = gaRepository;
            this._genericAttributeService = genericAttributeService;
            this._eventPublisher = eventPublisher;
            this._context = context;
        }

        #endregion Constructors

        #region Methods

        #region Users
        /// <summary>
        /// Gets all users
        /// </summary>
        /// <param name="registrationFrom">User registration from; null to load all users</param>
        /// <param name="registrationTo">User registration to; null to load all users</param>
        /// <param name="userRoleIds">A list of user role identifiers to filter by (at least one match); pass null or empty list in order to load all users; </param>
        /// <param name="email">Email; null to load all users</param>
        /// <param name="username">Username; null to load all users</param>
        /// <param name="firstName">First name; null to load all users</param>
        /// <param name="lastName">Last name; null to load all users</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all users</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all users</param>
        /// <param name="company">Company; null to load all users</param>
        /// <param name="phone">Phone; null to load all users</param>
        /// <param name="zipPostalCode">Phone; null to load all users</param>
        /// <param name="loadOnlyWithShoppingCart">Value indicating whther to load users only with shopping cart</param>
        /// <param name="sct">Value indicating what shopping cart type to filter; userd when 'loadOnlyWithShoppingCart' param is 'true'</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>User collection</returns>
        public virtual IPagedList<User> GetAllUsers(DateTime? registrationFrom,
            DateTime? registrationTo, int[] userRoleIds, string email, string username,
            string firstName, string lastName, int dayOfBirth, int monthOfBirth,
            string company, string phone, string zipPostalCode,
            int pageIndex, int pageSize)
        {
            var query = _userRepository.Table;
            if (registrationFrom.HasValue)
                query = query.Where(c => registrationFrom.Value <= c.CreatedOnUtc);
            if (registrationTo.HasValue)
                query = query.Where(c => registrationTo.Value >= c.CreatedOnUtc);
            query = query.Where(c => !c.Deleted);
            if (userRoleIds != null && userRoleIds.Length > 0)
                query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(userRoleIds).Count() > 0);
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));
            if (!String.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(x => x.FirstName.Contains(firstName));
            }
            if (!String.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(x => x.LastName.Contains(lastName));
            }
            //date of birth is stored as a string into database.
            //we also know that date of birth is stored in the following format YYYY-MM-DD (for example, 1983-02-18).
            //so let's search it as a string
            if (dayOfBirth > 0 && monthOfBirth > 0)
            {
                //both are specified
                string dateOfBirthStr = monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-" + dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                //EndsWith is not supported by SQL Server Compact
                //so let's use the following workaround http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00

                //we also cannot use Length function in SQL Server Compact (not supported in this context)
                //z.Attribute.Value.Length - dateOfBirthStr.Length = 5
                //dateOfBirthStr.Length = 5
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Substring(5, 5) == dateOfBirthStr))
                    .Select(z => z.User);
            }
            else if (dayOfBirth > 0)
            {
                //only day is specified
                string dateOfBirthStr = dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                //EndsWith is not supported by SQL Server Compact
                //so let's use the following workaround http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00

                //we also cannot use Length function in SQL Server Compact (not supported in this context)
                //z.Attribute.Value.Length - dateOfBirthStr.Length = 8
                //dateOfBirthStr.Length = 2
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Substring(8, 2) == dateOfBirthStr))
                    .Select(z => z.User);
            }
            else if (monthOfBirth > 0)
            {
                //only month is specified
                string dateOfBirthStr = "-" + monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-";
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Contains(dateOfBirthStr)))
                    .Select(z => z.User);
            }
            //search by company
            if (!String.IsNullOrWhiteSpace(company))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.Company &&
                        z.Attribute.Value.Contains(company)))
                    .Select(z => z.User);
            }
            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.Phone &&
                        z.Attribute.Value.Contains(phone)))
                    .Select(z => z.User);
            }
            //search by zip
            if (!String.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { User = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "User" &&
                        z.Attribute.Key == SystemUserAttributeNames.ZipPostalCode &&
                        z.Attribute.Value.Contains(zipPostalCode)))
                    .Select(z => z.User);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var users = new PagedList<User>(query, pageIndex, pageSize);
            return users;
        }

        /// <summary>
        /// Gets all users by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Users</returns>
        public virtual IPagedList<User> GetAllUsers(int pageIndex, int pageSize)
        {
            var query = _userRepository.Table;
            query = query.Where(c => !c.Deleted);
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var users = new PagedList<User>(query, pageIndex, pageSize);
            return users;
        }

        /// <summary>
        /// Gets all users by user format (including deleted ones)
        /// </summary>
        /// <param name="passwordFormat">Password format</param>
        /// <returns>Users</returns>
        public virtual IList<User> GetAllUsersByPasswordFormat(PasswordFormat passwordFormat)
        {
            int passwordFormatId = (int)passwordFormat;

            var query = _userRepository.Table;
            query = query.Where(c => c.PasswordFormatId == passwordFormatId);
            query = query.OrderByDescending(c => c.CreatedOnUtc);
            var users = query.ToList();
            return users;
        }

        /// <summary>
        /// Gets online users
        /// </summary>
        /// <param name="lastActivityFromUtc">User last activity date (from)</param>
        /// <param name="userRoleIds">A list of user role identifiers to filter by (at least one match); pass null or empty list in order to load all users; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>User collection</returns>
        public virtual IPagedList<User> GetOnlineUsers(DateTime lastActivityFromUtc, int[] userRoleIds, 
            int pageIndex, int pageSize)
        {
            var query = _userRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);
            if (userRoleIds != null && userRoleIds.Length > 0)
                query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Intersect(userRoleIds).Count() > 0);

            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var users = new PagedList<User>(query, pageIndex, pageSize);
            return users;
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="user">User</param>
        public virtual void DeleteUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (user.IsSystemAccount)
                throw new InSearchException(string.Format("System user account ({0}) could not be deleted", user.SystemName));

            user.Deleted = true;
            UpdateUser(user);
        }

        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>A user</returns>
        public virtual User GetUserById(int userId)
        {
            if (userId == 0)
                return null;

            var user = _userRepository.GetById(userId);
            return user;
        }

        /// <summary>
        /// Get users by identifiers
        /// </summary>
        /// <param name="userIds">User identifiers</param>
        /// <returns>Users</returns>
        public virtual IList<User> GetUsersByIds(int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return new List<User>();

            var query = from c in _userRepository.Table
                        where userIds.Contains(c.Id)
                        select c;
            var users = query.ToList();
            //sort by passed identifiers
            var sortedUsers = new List<User>();
            foreach (int id in userIds)
            {
                var user = users.Find(x => x.Id == id);
                if (user != null)
                    sortedUsers.Add(user);
            }
            return sortedUsers;
        }

        /// <summary>
        /// Gets a user by GUID
        /// </summary>
        /// <param name="userGuid">User GUID</param>
        /// <returns>A user</returns>
        public virtual User GetUserByGuid(Guid userGuid)
        {
            if (userGuid == Guid.Empty)
                return null;

            var query = from c in _userRepository.Table
                        where c.UserGuid == userGuid
                        orderby c.Id
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>User</returns>
        public virtual User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in _userRepository.Table
                        orderby c.Id
                        where c.Email == email
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get user by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>User</returns>
        public virtual User GetUserBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from c in _userRepository.Table
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User</returns>
        public virtual User GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in _userRepository.Table
                        orderby c.Id
                        where c.Username == username
                        select c;

            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Insert a guest user
        /// </summary>
        /// <returns>User</returns>
        public virtual User InsertGuestUser()
        {
            var user = new User()
            {
                UserGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            //add to 'Guests' role
            var guestRole = GetUserRoleBySystemName(SystemUserRoleNames.Guests);
            if (guestRole == null)
                throw new InSearchException("'Guests' role could not be loaded");
            user.UserRoles.Add(guestRole);

            _userRepository.Insert(user);

            return user;
        }

        /// <summary>
        /// Insert a user
        /// </summary>
        /// <param name="user">User</param>
        public virtual void InsertUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Insert(user);

            //event notification
            _eventPublisher.EntityInserted(user);
        }

        /// <summary>
        /// Updates the user
        /// </summary>
        /// <param name="user">User</param>
        public virtual void UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            _userRepository.Update(user);

            //event notification
            _eventPublisher.EntityUpdated(user);
        }

        /// <summary>
        /// Delete guest user records
        /// </summary>
        /// <param name="registrationFrom">User registration from; null to load all users</param>
        /// <param name="registrationTo">User registration to; null to load all users</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete users only without shopping cart</param>
        /// <returns>Number of deleted users</returns>
        public virtual int DeleteGuestUsers(DateTime? registrationFrom, DateTime? registrationTo)
        {
            var guestRole = GetUserRoleBySystemName(SystemUserRoleNames.Guests);
            if (guestRole == null)
                throw new InSearchException("'Guests' role could not be loaded");

            var parameters = new List<string>(); 

            var query = _userRepository.Table;
            if (registrationFrom.HasValue)
            {
                query = query.Where(c => registrationFrom.Value <= c.CreatedOnUtc);
                parameters.Add("{0}{1}'{2}'".FormatInvariant("U.CreatedOnUtc", "=>", registrationFrom.Value));
            }
            if (registrationTo.HasValue)
            {
                query = query.Where(c => registrationTo.Value >= c.CreatedOnUtc);
                parameters.Add("{0}{1}'{2}'".FormatInvariant("U.CreatedOnUtc", "<=", registrationTo.Value));
            }
            query = query.Where(c => c.UserRoles.Select(cr => cr.Id).Contains(guestRole.Id));
            
            //don't delete system accounts
            query = query.Where(c => !c.IsSystemAccount);
            var users = query.ToList();

            var command1 = DeleteAttributesGuestUserCommand.FormatInvariant("User", guestRole.Id);
            var command2 = DeleteGuestUserCommand.FormatInvariant(guestRole.Id);
            var joinedParameters = string.Join(" And ", parameters);
            if (!joinedParameters.IsNullOrEmpty())
            {
                command1 += " And " + joinedParameters;
                command2 += " And " + joinedParameters;
            }

            var numberOfDeletedUsers = 0;
            try
            {
                _context.SqlQuery<GenericAttribute>(command1);
                _context.SqlQuery<User>(command2);
                numberOfDeletedUsers = users.Count();
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }

            //int numberOfDeletedUsers = 0;
            //foreach (var c in users)
            //{
            //    try
            //    {
            //        //delete from database
            //        //_userRepository.Delete(c);
            //        numberOfDeletedUsers++;

            //        //delete attributes
            //        var attributes = _genericAttributeService.GetAttributesForEntity(c.Id, "User");
            //        foreach (var attribute in attributes)
            //        {
            //            _genericAttributeService.DeleteAttribute(attribute);
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        Debug.WriteLine(exc);
            //    }
            //}

            return numberOfDeletedUsers;
        }
        #endregion Users

        #region User roles
        /// <summary>
        /// Delete a user role
        /// </summary>
        /// <param name="userRole">User role</param>
        public virtual void DeleteUserRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException("userRole");

            if (userRole.IsSystemRole)
                throw new InSearchException("System role could not be deleted");

            _userRoleRepository.Delete(userRole);

            _cacheManager.RemoveByPattern(USERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(userRole);
        }

        /// <summary>
        /// Gets a user role
        /// </summary>
        /// <param name="userRoleId">User role identifier</param>
        /// <returns>User role</returns>
        public virtual UserRole GetUserRoleById(int userRoleId)
        {
            if (userRoleId == 0)
                return null;

            return _userRoleRepository.GetById(userRoleId);
        }

        /// <summary>
        /// Gets a user role
        /// </summary>
        /// <param name="systemName">User role system name</param>
        /// <returns>User role</returns>
        public virtual UserRole GetUserRoleBySystemName(string systemName)
        {
            if (String.IsNullOrWhiteSpace(systemName))
                return null;

            string key = string.Format(USERROLES_BY_SYSTEMNAME_KEY, systemName);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _userRoleRepository.Table
                            orderby cr.Id
                            where cr.SystemName == systemName
                            select cr;
                var userRole = query.FirstOrDefault();
                return userRole;
            });
        }

        /// <summary>
        /// Gets all user roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>User role collection</returns>
        public virtual IList<UserRole> GetAllUserRoles(bool showHidden = false)
        {
            string key = string.Format(USERROLES_ALL_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _userRoleRepository.Table
                            orderby cr.Name
                            where (showHidden || cr.Active)
                            select cr;
                var userRoles = query.ToList();
                return userRoles;
            });
        }

        /// <summary>
        /// Inserts a user role
        /// </summary>
        /// <param name="userRole">User role</param>
        public virtual void InsertUserRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException("userRole");

            _userRoleRepository.Insert(userRole);

            _cacheManager.RemoveByPattern(USERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(userRole);
        }

        /// <summary>
        /// Updates the user role
        /// </summary>
        /// <param name="userRole">User role</param>
        public virtual void UpdateUserRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException("userRole");

            _userRoleRepository.Update(userRole);

            _cacheManager.RemoveByPattern(USERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(userRole);
        }
        #endregion  User roles
        #endregion Methods
    }
}
