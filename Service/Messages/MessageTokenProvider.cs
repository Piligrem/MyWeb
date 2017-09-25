using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using InSearch.Core;
using InSearch.Core.Domain;
using InSearch.Core.Domain.Common;
using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Messages;
using InSearch.Services.Common;
using InSearch.Services.Users;
using InSearch.Core.Events;
using InSearch.Services.Helpers;
using InSearch.Services.Localization;
using InSearch.Services.Media;
using InSearch.Core.Domain.Forums;
using InSearch.Services.Seo;
using InSearch.Services.Forums;

namespace InSearch.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;

        private readonly SiteInformationSettings _siteInfoSettings;
        private readonly SiteSettings _siteSettings;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly EmailAccountSettings _emailAccountSettings;

        private readonly IEventPublisher _eventPublisher;

        private readonly CompanyInformationSettings _companyInfoSettings;
        private readonly ContactDataSettings _contactDataSettings;
        private readonly BankConnectionSettings _bankConnectionSettings;
        #endregion

        #region Constructors
        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IEmailAccountService emailAccountService, EmailAccountSettings emailAccountSettings,
            IWebHelper webHelper, 
            IWorkContext workContext, IDownloadService downloadService,
            SiteInformationSettings siteInfoSettings, CompanyInformationSettings companyInfoSettings,
            ContactDataSettings contactDataSettings, BankConnectionSettings bankConnectionSettings,
            MessageTemplatesSettings templatesSettings, SiteSettings siteSettings,
            IEventPublisher eventPublisher)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._downloadService = downloadService;
            this._siteSettings = siteSettings;

            this._siteInfoSettings = siteInfoSettings;
            this._templatesSettings = templatesSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
            
            this._companyInfoSettings = companyInfoSettings;
            this._contactDataSettings = contactDataSettings;
            this._bankConnectionSettings = bankConnectionSettings;
        }
        #endregion Constructors

        #region Utilities
        protected virtual string GetSupplierIdentification()
        {
            var result = "";
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" class=\"supplier-identification\">");
            sb.AppendLine("<tr valign=\"top\">");
			
            sb.AppendLine("<td class=\"smaller\" width=\"25%\">");

            sb.AppendLine(String.Format("{0} <br>", _companyInfoSettings.CompanyName ));

            if (!String.IsNullOrEmpty(_companyInfoSettings.Salutation)) 
            {
                sb.AppendLine(_companyInfoSettings.Salutation);
            }
            if (!String.IsNullOrEmpty(_companyInfoSettings.Title)) 
            {
                sb.AppendLine(_companyInfoSettings.Title);
            }
            if (!String.IsNullOrEmpty(_companyInfoSettings.Firstname)) 
            {
                sb.AppendLine(String.Format("{0} ", _companyInfoSettings.Firstname));
            }
            if (!String.IsNullOrEmpty(_companyInfoSettings.Lastname)) 
            {
                sb.AppendLine(_companyInfoSettings.Lastname);
            }
            sb.AppendLine("<br>");

            if (!String.IsNullOrEmpty(_companyInfoSettings.Street)) 
            {
                sb.AppendLine(String.Format("{0} {1}<br>", _companyInfoSettings.Street, _companyInfoSettings.Street2));
            }
			if (!String.IsNullOrEmpty(_companyInfoSettings.ZipCode) || !String.IsNullOrEmpty(_companyInfoSettings.City)) 
            {
                sb.AppendLine(String.Format("{0} {1}<br>", _companyInfoSettings.ZipCode, _companyInfoSettings.City));
            }	
			if (!String.IsNullOrEmpty(_companyInfoSettings.CountryName)) 
            {
                sb.AppendLine(_companyInfoSettings.CountryName);

                if(!String.IsNullOrEmpty(_companyInfoSettings.Region))
                {
                    sb.AppendLine(String.Format(", {0}", _companyInfoSettings.Region));
                }
                sb.AppendLine("<br>");
            }				

            sb.AppendLine("<td/>");

            sb.AppendLine("<td class=\"smaller\" width=\"50%\">");

            if (!String.IsNullOrEmpty(_siteSettings.Url)) 
            {
                sb.AppendLine(String.Format("Url: <a href=\"{0}\">{0}</a><br>", _siteSettings.Url));
            }
            if (!String.IsNullOrEmpty(_contactDataSettings.CompanyEmailAddress)) 
            {
                sb.AppendLine(String.Format("Mail: {0}<br>", _contactDataSettings.CompanyEmailAddress));
            }
			if (!String.IsNullOrEmpty(_contactDataSettings.CompanyTelephoneNumber)) 
            {
                sb.AppendLine(String.Format("Fon: {0}<br>", _contactDataSettings.CompanyTelephoneNumber));
            }
            if (!String.IsNullOrEmpty(_contactDataSettings.CompanyFaxNumber)) 
            {
                sb.AppendLine(String.Format("Fax: {0}<br>", _contactDataSettings.CompanyFaxNumber));
            }

            sb.AppendLine("<td/>");

            sb.AppendLine("<td class=\"smaller\" width=\"25%\">");

            if (!String.IsNullOrEmpty(_bankConnectionSettings.Bankname)) 
            {
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Bankname));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.Bankcode)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Bankcode));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.AccountNumber)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.AccountNumber));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.AccountHolder)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.AccountHolder));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.Iban)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Iban));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.Bic)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Bic));
            }

            sb.AppendLine("<td/>");

            sb.AppendLine("<tr/>");
            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }
        #endregion

        #region Methods
		public virtual void AddSiteTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Site.Name", _siteSettings.Name));
            tokens.Add(new Token("Site.URL", _siteSettings.Url, true));
            var defaultEmailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (defaultEmailAccount == null)
                defaultEmailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            tokens.Add(new Token("Site.SupplierIdentification", GetSupplierIdentification(), true));
            tokens.Add(new Token("Site.Email", defaultEmailAccount.Email));
        }
        public virtual void AddCompanyTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Company.CompanyName", _companyInfoSettings.CompanyName));
            tokens.Add(new Token("Company.Salutation", _companyInfoSettings.Salutation));
            tokens.Add(new Token("Company.Title", _companyInfoSettings.Title));
            tokens.Add(new Token("Company.Firstname", _companyInfoSettings.Firstname));
            tokens.Add(new Token("Company.Lastname", _companyInfoSettings.Lastname));
            tokens.Add(new Token("Company.CompanyManagementDescription", _companyInfoSettings.CompanyManagementDescription));
            tokens.Add(new Token("Company.CompanyManagement", _companyInfoSettings.CompanyManagement));
            tokens.Add(new Token("Company.Street", _companyInfoSettings.Street));
            tokens.Add(new Token("Company.Street2", _companyInfoSettings.Street2));
            tokens.Add(new Token("Company.ZipCode", _companyInfoSettings.ZipCode));
            tokens.Add(new Token("Company.City", _companyInfoSettings.City));
            tokens.Add(new Token("Company.CountryName", _companyInfoSettings.CountryName));
            tokens.Add(new Token("Company.Region", _companyInfoSettings.Region));
            tokens.Add(new Token("Company.VatId", _companyInfoSettings.VatId));
            tokens.Add(new Token("Company.CommercialRegister", _companyInfoSettings.CommercialRegister));
            tokens.Add(new Token("Company.TaxNumber", _companyInfoSettings.TaxNumber));
        }
        public virtual void AddBankConnectionTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Bank.Bankname", _bankConnectionSettings.Bankname));
            tokens.Add(new Token("Bank.Bankcode", _bankConnectionSettings.Bankcode));
            tokens.Add(new Token("Bank.AccountNumber", _bankConnectionSettings.AccountNumber));
            tokens.Add(new Token("Bank.AccountHolder", _bankConnectionSettings.AccountHolder));
            tokens.Add(new Token("Bank.Iban", _bankConnectionSettings.Iban));
            tokens.Add(new Token("Bank.Bic", _bankConnectionSettings.Bic));
        }
        public virtual void AddContactDataTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Contact.CompanyTelephoneNumber", _contactDataSettings.CompanyTelephoneNumber));
            tokens.Add(new Token("Contact.HotlineTelephoneNumber", _contactDataSettings.HotlineTelephoneNumber));
            tokens.Add(new Token("Contact.MobileTelephoneNumber", _contactDataSettings.MobileTelephoneNumber));
            tokens.Add(new Token("Contact.CompanyFaxNumber", _contactDataSettings.CompanyFaxNumber));
            tokens.Add(new Token("Contact.CompanyEmailAddress", _contactDataSettings.CompanyEmailAddress));
            tokens.Add(new Token("Contact.WebmasterEmailAddress", _contactDataSettings.WebmasterEmailAddress));
            tokens.Add(new Token("Contact.SupportEmailAddress", _contactDataSettings.SupportEmailAddress));
            tokens.Add(new Token("Contact.ContactEmailAddress", _contactDataSettings.ContactEmailAddress));
        }
        public virtual void AddUserTokens(IList<Token> tokens, User user)
        {
            tokens.Add(new Token("User.Email", user.Email));
            tokens.Add(new Token("User.Username", user.Username));
            tokens.Add(new Token("User.FullName", user.GetFullName()));

            //note: we do not use SEO friendly URLS because we can get errors caused by having .(dot) in the URL (from the email address)
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", _siteSettings.Url,
                user.GetAttribute<string>(SystemUserAttributeNames.PasswordRecoveryToken), HttpUtility.UrlEncode(user.Email));

            string accountActivationUrl = string.Format("{0}user/activation?token={1}&email={2}", _siteSettings.Url,
                user.GetAttribute<string>(SystemUserAttributeNames.AccountActivationToken), HttpUtility.UrlEncode(user.Email));

            tokens.Add(new Token("User.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("User.AccountActivationURL", accountActivationUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(user, tokens);
        }
        public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));


            const string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";

            var activationUrl = String.Format(urlFormat, _siteSettings.Url, subscription.NewsLetterSubscriptionGuid, "true");
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, _siteSettings.Url, subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }

        #region Forum
        public virtual void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic, int? friendlyForumTopicPageIndex = null, long? appendedPostIdentifierAnchor = null)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string topicUrl = null;
            if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
                topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", _webHelper.GetSiteLocation(false), forumTopic.Id, forumTopic.GetSeName(), friendlyForumTopicPageIndex.Value);
            else
                topicUrl = string.Format("{0}boards/topic/{1}/{2}", _webHelper.GetSiteLocation(false), forumTopic.Id, forumTopic.GetSeName());
            if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
                topicUrl = string.Format("{0}#{1}", topicUrl, appendedPostIdentifierAnchor.Value);
            tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
            tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));

            //event notification
            _eventPublisher.EntityTokensAdded(forumTopic, tokens);
        }
        public virtual void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost)
        {
            tokens.Add(new Token("Forums.PostAuthor", forumPost.User.FormatUserName()));
            tokens.Add(new Token("Forums.PostBody", forumPost.FormatPostText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(forumPost, tokens);
        }
        public virtual void AddForumTokens(IList<Token> tokens, Forum forum)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var forumUrl = string.Format("{0}boards/forum/{1}/{2}", _webHelper.GetSiteLocation(false), forum.Id, forum.GetSeName());
            tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
            tokens.Add(new Token("Forums.ForumName", forum.Name));

            //event notification
            _eventPublisher.EntityTokensAdded(forum, tokens);
        }
        #endregion Forum

        public virtual void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage)
        {
            tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
            tokens.Add(new Token("PrivateMessage.Text", privateMessage.FormatPrivateMessageText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(privateMessage, tokens);
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = new List<string>()
            {
                "%Site.Name%",
                "%Site.URL%",
                "%Site.Email%",
                "%User.Email%", 
                "%User.Username%", 
                "%User.FullName%", 
                "%User.PasswordRecoveryURL%", 
                "%User.AccountActivationURL%", 
                "%NewsLetterSubscription.Email%", 
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%", 
                "%Site.SupplierIdentification%",
            };
            return allowedTokens.ToArray();
        }
        #endregion
    }
}
