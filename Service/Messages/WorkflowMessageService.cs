using System;
using System.Collections.Generic;
using System.Linq;
using InSearch.Core;
using InSearch.Core.Domain.Common;
using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Messages;
using InSearch.Services.Users;
using InSearch.Core.Events;
using InSearch.Services.Localization;
using InSearch.Core.Domain.Forums;

namespace InSearch.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILanguageService _languageService;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;

        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;
        private readonly ISiteContext _siteContext;
        #endregion Fields

        #region Constructors
        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService, ILanguageService languageService,
            ITokenizer tokenizer, IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            EmailAccountSettings emailAccountSettings,
            IEventPublisher eventPublisher, ISiteContext siteContext,
            IWorkContext workContext)
        {
            this._messageTemplateService = messageTemplateService;
            this._queuedEmailService = queuedEmailService;
            this._languageService = languageService;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;

            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
            this._workContext = workContext;
            this._siteContext = siteContext;
        }
        #endregion Constructors

        #region Utilities
        protected int SendNotification(
            MessageTemplate messageTemplate, EmailAccount emailAccount,
            int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
			string replyTo = null, string replyToName = null)
        {
            // retrieve localized message template data
            var bcc = messageTemplate.GetLocalized((mt) => mt.BccEmailAddresses, languageId);
            var subject = messageTemplate.GetLocalized((mt) => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized((mt) => mt.Body, languageId);

            // Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var email = new QueuedEmail()
            {
                Priority = 5,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                CC = string.Empty,
                Bcc = bcc,
				ReplyTo = replyTo,
				ReplyToName = replyToName,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }

        protected MessageTemplate GetLocalizedActiveMessageTemplate(string messageTemplateName, int languageId)
        {
			//TODO remove languageId parameter
			var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName);

			//no template found
			if (messageTemplate == null)
				return null;

			//ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }
        protected EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccounId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccounId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            return emailAccount;
        }
        private Tuple<string, string> GetReplyToEmail(User user)
        {
			if (user == null || user.Email.IsEmpty())
				return new Tuple<string, string>(null, null);

			string email = user.Email;
			string name = GetDisplayNameForUser(user);

			return new Tuple<string, string>(email, name);
        }
        private string GetDisplayNameForUser(User user)
        {
            if (user == null)
                return string.Empty;

            Func<Address, string> getName = (address) => {
                if (address == null)
                    return null;

                string result = string.Empty;
                if (address.FirstName.HasValue() || address.LastName.HasValue())
                {
                    result = string.Format("{0} {1}", address.FirstName, address.LastName).Trim();
                }

                if (address.Company.HasValue())
                {
                    result = string.Concat(result, result.HasValue() ? ", " : "", address.Company);
                }

                return result;
            };

            string name = getName(user.Addresses.FirstOrDefault());

            name = name.TrimSafe().NullEmpty();

            return name ?? user.Username.EmptyNull();
        }
		protected int EnsureLanguageIsActive(int languageId)
        {
			//load language by specified ID
            var language = _languageService.GetLanguageById(languageId);

            if (language == null || !language.Published)
			{
				//load any language from the specified store
				language = _languageService.GetAllLanguages().FirstOrDefault();
			}
			if (language == null || !language.Published)
			{
				//load any language
				language = _languageService.GetAllLanguages().FirstOrDefault();
			}

			if (language == null)
				throw new Exception("No active language could be loaded");
            return language.Id;
        }
        #endregion Utilities

        #region Methods

        #region User workflow
        /// <summary>
        /// Sends 'New user' notification message to a store owner
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserRegisteredNotificationMessage(User user, int languageId)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            languageId = EnsureLanguageIsActive(languageId);

			var messageTemplate = GetLocalizedActiveMessageTemplate("NewUser.Notification", languageId);
            if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddSiteTokens(tokens);
			_messageTokenProvider.AddUserTokens(tokens, user);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            // use user email as reply address
			var replyTo = GetReplyToEmail(user);

            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName,
				replyTo.Item1, replyTo.Item2);
        }

        /// <summary>
        /// Sends a welcome message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserWelcomeMessage(User user, int languageId)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            languageId = EnsureLanguageIsActive(languageId);

			var messageTemplate = GetLocalizedActiveMessageTemplate("User.WelcomeMessage", languageId);
            if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddSiteTokens(tokens);
			_messageTokenProvider.AddUserTokens(tokens, user);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = user.Email;
            var toName = user.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends an email validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserEmailValidationMessage(User user, int languageId)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            languageId = EnsureLanguageIsActive(languageId);

			var messageTemplate = GetLocalizedActiveMessageTemplate("User.EmailValidationMessage", languageId);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddSiteTokens(tokens);
			_messageTokenProvider.AddUserTokens(tokens, user);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = user.Email;
            var toName = user.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        /// <summary>
        /// Sends password recovery message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendUserPasswordRecoveryMessage(User user, int languageId)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            languageId = EnsureLanguageIsActive(languageId);

			var messageTemplate = GetLocalizedActiveMessageTemplate("User.PasswordRecovery", languageId);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddSiteTokens(tokens);
			_messageTokenProvider.AddUserTokens(tokens, user);

            //event notification
			_eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = user.Email;
            var toName = user.GetFullName();
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }
        #endregion User workflow

        #region Newsletter workflow
        /// <summary>
        /// Sends a newsletter subscription activation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            languageId = EnsureLanguageIsActive(languageId);

			var messageTemplate = GetLocalizedActiveMessageTemplate("NewsLetterSubscription.ActivationMessage", languageId);
			if (messageTemplate == null)
                return 0;

			//tokens
			var tokens = new List<Token>();
			_messageTokenProvider.AddSiteTokens(tokens);
			_messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNewsLetterSubscriptionDeactivationMessage(NewsLetterSubscription subscription, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

			languageId = EnsureLanguageIsActive(languageId);
			
            var messageTemplate = GetLocalizedActiveMessageTemplate("NewsLetterSubscription.DeactivationMessage", languageId);
            if (messageTemplate == null)
                return 0;

			var tokens = new List<Token>();
			_messageTokenProvider.AddSiteTokens(tokens);
            _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }
        #endregion Newsletter workflow

        #region Forum Notifications
        /// <summary>
        /// Sends a forum subscription message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumTopicMessage(User user, ForumTopic forumTopic, Forum forum, int languageId)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var messageTemplate = GetLocalizedActiveMessageTemplate("Forums.NewForumTopic", languageId);
            if (messageTemplate == null)
            {
                return 0;
            }

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddSiteTokens(tokens);
            _messageTokenProvider.AddForumTopicTokens(tokens, forumTopic);
            _messageTokenProvider.AddForumTokens(tokens, forumTopic.Forum);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = user.Email;
            var toName = user.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a forum subscription message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendNewForumPostMessage(User user, ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, int languageId)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var messageTemplate = GetLocalizedActiveMessageTemplate("Forums.NewForumPost", languageId);
            if (messageTemplate == null)
            {
                return 0;
            }

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddSiteTokens(tokens);
            _messageTokenProvider.AddForumPostTokens(tokens, forumPost);
            _messageTokenProvider.AddForumTopicTokens(tokens, forumPost.ForumTopic, friendlyForumTopicPageIndex, forumPost.Id);
            _messageTokenProvider.AddForumTokens(tokens, forumPost.ForumTopic.Forum);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = user.Email;
            var toName = user.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public int SendPrivateMessageNotification(PrivateMessage privateMessage, int languageId)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            var messageTemplate = GetLocalizedActiveMessageTemplate("User.NewPM", languageId);
            if (messageTemplate == null)
            {
                return 0;
            }

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddSiteTokens(tokens);
            _messageTokenProvider.AddPrivateMessageTokens(tokens, privateMessage);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = privateMessage.ToUser.Email;
            var toName = privateMessage.ToUser.GetFullName();

            return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }
        #endregion Forum Notifications

        #region Misc
        public virtual int SendGenericMessage(string messageTemplateName, Action<GenericMessageContext> cfg)
        {
            Guard.ArgumentNotNull(() => cfg);
            Guard.ArgumentNotEmpty(() => messageTemplateName);

            var ctx = new GenericMessageContext();
            ctx.MessagenTokenProvider = _messageTokenProvider;

            cfg(ctx);

            if (!ctx.LanguageId.HasValue)
            {
                ctx.LanguageId = _workContext.WorkingLanguage.Id;
            }

            if (ctx.User == null)
            {
                ctx.User = _workContext.CurrentUser;
            }

            if (ctx.User.IsSystemAccount)
                return 0;

            _messageTokenProvider.AddUserTokens(ctx.Tokens, ctx.User);
            _messageTokenProvider.AddSiteTokens(ctx.Tokens);

            ctx.LanguageId = EnsureLanguageIsActive(ctx.LanguageId.Value);

            var messageTemplate = GetLocalizedActiveMessageTemplate(messageTemplateName, ctx.LanguageId.Value);
            if (messageTemplate == null)
                return 0;

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, ctx.LanguageId.Value);
            var toEmail = ctx.ToEmail.HasValue() ? ctx.ToEmail : emailAccount.Email;
            var toName = ctx.ToName.HasValue() ? ctx.ToName : emailAccount.DisplayName;

            if (ctx.ReplyToUser && ctx.User != null)
            {
                // use user email as reply address
				var replyTo = GetReplyToEmail(ctx.User);
				ctx.ReplyToEmail = replyTo.Item1;
				ctx.ReplyToName = replyTo.Item2;
            }

			return SendNotification(messageTemplate, emailAccount, ctx.LanguageId.Value, ctx.Tokens, toEmail, toName, ctx.ReplyToEmail, ctx.ReplyToName);
        }
        #endregion Misc

        #endregion Methods
    }
}
