using System;
using System.Linq;
using InSearch.Core;
using InSearch.Core.Domain.Users;
//using InSearch.Services.Localization;
using InSearch.Services.Security;
using InSearch.Services.Localization;

namespace InSearch.Services.Users
{
    public partial class UserRegistrationService : IUserRegistrationService
    {
        #region Fields
        private readonly IUserService _userService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILocalizationService _localizationService;
        private readonly UserSettings _userSettings;
        #endregion

        #region Construstors

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService">User service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="newsLetterSubscriptionService">Newsletter subscription service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="userSettings">User settings</param>
        public UserRegistrationService(IUserService userService,
            IEncryptionService encryptionService,
            ILocalizationService localizationService,
            UserSettings userSettings)
        {
            this._userService = userService;
            this._encryptionService = encryptionService;
            this._localizationService = localizationService;
            this._userSettings = userSettings;
        }
        #endregion Construstors

        #region Methods
        /// <summary>
        /// Validate user
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        public virtual bool ValidateUser(string usernameOrEmail, string password)
        {
            User user = null;
            if (_userSettings.UsernamesEnabled)
                user = _userService.GetUserByUsername(usernameOrEmail);
            else
                user = _userService.GetUserByEmail(usernameOrEmail);

            if (user == null || user.Deleted || !user.Active)
                return false;

            //only registered can login
            if (!user.IsRegistered())
                return false;

            string pwd = "";
            switch (user.PasswordFormat)
            {
                case PasswordFormat.Encrypted:
                    pwd = _encryptionService.EncryptText(password);
                    break;
                case PasswordFormat.Hashed:
                    pwd = _encryptionService.CreatePasswordHash(password, user.PasswordSalt, _userSettings.HashedPasswordFormat);
                    break;
                default:
                    pwd = password;
                    break;
            }

            bool isValid = pwd == user.Password;

            //save last login date
            if (isValid)
            {
                user.LastLoginDateUtc = DateTime.UtcNow;
                _userService.UpdateUser(user);
            }
            //else
            //{
            //    user.FailedPasswordAttemptCount++;
            //    UpdateUser(user);
            //}

            return isValid;
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual UserRegistrationResult RegisterUser(UserRegistrationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.User == null)
                throw new ArgumentException("Can't load current user");

            var result = new UserRegistrationResult();
            if (request.User.IsSearchEngineAccount())
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.SearchEngineCannotRegistered"));
                return result;
            }
            if (request.User.IsBackgroundTaskAccount())
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.BackgroundTaskAccountCannotRegistered"));
                return result;
            }
            if (request.User.IsRegistered())
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.CurrentUserAlredyRegistered"));
                return result;
            }
            if (String.IsNullOrEmpty(request.Email))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailIsNotProvided"));
                return result;
            }
            if (!request.Email.IsEmail())
            {
                result.AddError(_localizationService.GetResource("Common.WrongEmail"));
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.PasswordIsNotProvided"));
                return result;
            }
            if (_userSettings.UsernamesEnabled)
            {
                if (String.IsNullOrEmpty(request.Username))
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameIsNotProvided"));
                    return result;
                }
            }

            //validate unique user
            if (_userService.GetUserByEmail(request.Email) != null)
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.EmailAlreadyExists"));
                return result;
            }
            if (_userSettings.UsernamesEnabled)
            {
                if (_userService.GetUserByUsername(request.Username) != null)
                {
                    result.AddError(_localizationService.GetResource("Account.Register.Errors.UsernameAlreadyExists"));
                    return result;
                }
            }

            if (String.IsNullOrEmpty(request.FirstName))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.FirstNameIsNotProvided"));
                return result;
            }
            if (String.IsNullOrEmpty(request.LastName))
            {
                result.AddError(_localizationService.GetResource("Account.Register.Errors.LastNameIsNotProvided"));
                return result;
            }

            //at this point request is valid
            request.User.Username = request.Username;
            request.User.Email = request.Email;
            request.User.PasswordFormat = request.PasswordFormat;

            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        request.User.Password = request.Password;
                    }
                    break;
                case PasswordFormat.Encrypted:
                    {
                        request.User.Password = _encryptionService.EncryptText(request.Password);
                    }
                    break;
                case PasswordFormat.Hashed:
                    {
                        string saltKey = _encryptionService.CreateSaltKey(5);
                        request.User.PasswordSalt = saltKey;
                        request.User.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _userSettings.HashedPasswordFormat);
                    }
                    break;
                default:
                    break;
            }

            request.User.Active = request.IsApproved;

            //add to 'Registered' role
            var registeredRole = _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered);
            if (registeredRole == null)
                throw new InSearchException("'Registered' role could not be loaded");
            request.User.UserRoles.Add(registeredRole);
            //remove from 'Guests' role
            var guestRole = request.User.UserRoles.FirstOrDefault(cr => cr.Name == SystemUserRoleNames.Guests);
            if (guestRole != null)
                request.User.UserRoles.Remove(guestRole);

            request.FirstName = request.FirstName;
            request.LastName = request.LastName;

            _userService.UpdateUser(request.User);
            return result;
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual PasswordChangeResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var result = new PasswordChangeResult();
            if (String.IsNullOrWhiteSpace(request.Email))
            {
                //result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailIsNotProvided"));
                result.AddError("Email is not entered");
                return result;
            }
            if (String.IsNullOrWhiteSpace(request.NewPassword))
            {
                //result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.PasswordIsNotProvided"));
                result.AddError("Password is not entered");
                return result;
            }

            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                //result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.EmailNotFound"));
                result.AddError("The specified email could not be found");
                return result;
            }


            var requestIsValid = false;
            if (request.ValidateRequest)
            {
                //password
                string oldPwd = "";
                switch (user.PasswordFormat)
                {
                    case PasswordFormat.Encrypted:
                        oldPwd = _encryptionService.EncryptText(request.OldPassword);
                        break;
                    case PasswordFormat.Hashed:
                        oldPwd = _encryptionService.CreatePasswordHash(request.OldPassword, user.PasswordSalt, _userSettings.HashedPasswordFormat);
                        break;
                    default:
                        oldPwd = request.OldPassword;
                        break;
                }

                bool oldPasswordIsValid = oldPwd == user.Password;
                if (!oldPasswordIsValid)
                    //result.AddError(_localizationService.GetResource("Account.ChangePassword.Errors.OldPasswordDoesntMatch"));
                    result.AddError("Old password doesn't match");

                if (oldPasswordIsValid)
                    requestIsValid = true;
            }
            else
                requestIsValid = true;


            //at this point request is valid
            if (requestIsValid)
            {
                switch (request.NewPasswordFormat)
                {
                    case PasswordFormat.Clear:
                        {
                            user.Password = request.NewPassword;
                        }
                        break;
                    case PasswordFormat.Encrypted:
                        {
                            user.Password = _encryptionService.EncryptText(request.NewPassword);
                        }
                        break;
                    case PasswordFormat.Hashed:
                        {
                            string saltKey = _encryptionService.CreateSaltKey(5);
                            user.PasswordSalt = saltKey;
                            user.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _userSettings.HashedPasswordFormat);
                        }
                        break;
                    default:
                        break;
                }
                user.PasswordFormat = request.NewPasswordFormat;
                _userService.UpdateUser(user);
            }

            return result;
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="newEmail">New email</param>
        public virtual void SetEmail(User user, string newEmail)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            newEmail = newEmail.Trim();
            string oldEmail = user.Email;

            if (!newEmail.IsEmail())
                //throw new InSearchException(_localizationService.GetResource("Account.EmailUsernameErrors.NewEmailIsNotValid"));
                throw new InSearchException("New email is not valid");

            if (newEmail.Length > 100)
                //throw new InSearchException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailTooLong"));
                throw new InSearchException("E-mail address is too long");

            var user2 = _userService.GetUserByEmail(newEmail);
            if (user2 != null && user.Id != user2.Id)
                //throw new InSearchException(_localizationService.GetResource("Account.EmailUsernameErrors.EmailAlreadyExists"));
                throw new InSearchException("The e-mail address is already in use");

            user.Email = newEmail;
            _userService.UpdateUser(user);
        }

        /// <summary>
        /// Sets a user username
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="newUsername">New Username</param>
        public virtual void SetUsername(User user, string newUsername)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            if (!_userSettings.UsernamesEnabled)
                throw new InSearchException("Usernames are disabled");

            if (!_userSettings.AllowUsersToChangeUsernames)
                throw new InSearchException("Changing usernames is not allowed");

            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                //throw new InSearchException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameTooLong"));
                throw new InSearchException("Username is too long");

            var user2 = _userService.GetUserByUsername(newUsername);
            if (user2 != null && user.Id != user2.Id)
                //throw new InSearchException(_localizationService.GetResource("Account.EmailUsernameErrors.UsernameAlreadyExists"));
                throw new InSearchException("The username is already in use");

            user.Username = newUsername;
            _userService.UpdateUser(user);
        }

        public virtual void SetFirstLastNames(User user, string newFirstName, string newLastName)
        {
            if (newFirstName.IsNullOrEmpty() || newLastName.IsNullOrEmpty())
                throw new ArgumentNullException("fistName or lastName");

            var firstName = newFirstName.Trim();
            var lastName = newLastName.Trim();

            if (firstName.Length > 250)
                firstName.Substring(0, 249);
            if (lastName.Length > 250)
                lastName.Substring(0, 249);

            user.FirstName = firstName;
            user.LastName = lastName;

            _userService.UpdateUser(user);
        }
        #endregion Methods
    }
}
