using System;
using System.Collections.Generic;
using System.Linq;
using InSearch.Core.Caching;
using InSearch.Core.Data;
using InSearch.Core.Domain.Messages;
using InSearch.Core.Events;
using InSearch.Services.Localization;

namespace InSearch.Services.Messages
{
    public partial class MessageTemplateService: IMessageTemplateService
    {
        #region Constants

        private const string MESSAGETEMPLATES_ALL_KEY = "InSearch.messagetemplate.all";
        private const string MESSAGETEMPLATES_BY_NAME_KEY = "InSearch.messagetemplate.name-{0}";
        private const string MESSAGETEMPLATES_PATTERN_KEY = "InSearch.messagetemplate.";

        #endregion

        #region Fields

        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
		private readonly ILanguageService _languageService;
		private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Constructors
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
		/// <param name="storeMappingRepository">Store mapping repository</param>
		/// <param name="languageService">Language service</param>
		/// <param name="localizedEntityService">Localized entity service</param>
		/// <param name="storeMappingService">Store mapping service</param>
        /// <param name="messageTemplateRepository">Message template repository</param>
        /// <param name="eventPublisher">Event published</param>
        public MessageTemplateService(ICacheManager cacheManager,
			ILanguageService languageService,
			ILocalizedEntityService localizedEntityService,
            IRepository<MessageTemplate> messageTemplateRepository,
            IEventPublisher eventPublisher)
        {
			this._cacheManager = cacheManager;
			this._languageService = languageService;
			this._localizedEntityService = localizedEntityService;
			this._messageTemplateRepository = messageTemplateRepository;
			this._eventPublisher = eventPublisher;

			this.QuerySettings = DbQuerySettings.Default;
		}
        #endregion Constructors

        public DbQuerySettings QuerySettings { get; set; }

        #region Methods
        /// <summary>
		/// Delete a message template
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		public virtual void DeleteMessageTemplate(MessageTemplate messageTemplate)
		{
			if (messageTemplate == null)
				throw new ArgumentNullException("messageTemplate");

			_messageTemplateRepository.Delete(messageTemplate);

			_cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityDeleted(messageTemplate);
		}

        /// <summary>
        /// Inserts a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void InsertMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            _messageTemplateRepository.Insert(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(messageTemplate);
        }

        /// <summary>
        /// Updates a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void UpdateMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");

            _messageTemplateRepository.Update(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(messageTemplate);
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <returns>Message template</returns>
        public virtual MessageTemplate GetMessageTemplateById(int messageTemplateId)
        {
            if (messageTemplateId == 0)
                return null;

            return _messageTemplateRepository.GetById(messageTemplateId);
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <returns>Message template</returns>
		public virtual MessageTemplate GetMessageTemplateByName(string messageTemplateName)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException("messageTemplateName");

            string key = string.Format(MESSAGETEMPLATES_BY_NAME_KEY, messageTemplateName);
            return _cacheManager.Get(key, () =>
            {
				var query = _messageTemplateRepository.Table;
				query = query.Where(t => t.Name == messageTemplateName);
				query = query.OrderBy(t => t.Id);
				var templates = query.ToList();

                return templates.FirstOrDefault();
            });

        }

        /// <summary>
        /// Gets all message templates
        /// </summary>
        /// <returns>Message template list</returns>
		public virtual IList<MessageTemplate> GetAllMessageTemplates()
        {
			string key = MESSAGETEMPLATES_ALL_KEY;
			return _cacheManager.Get(key, () =>
            {
				var query = _messageTemplateRepository.Table;
				query = query.OrderBy(t => t.Name);

				return query.ToList();
            });
        }

		/// <summary>
		/// Create a copy of message template with all depended data
		/// </summary>
		/// <param name="messageTemplate">Message template</param>
		/// <returns>Message template copy</returns>
		public virtual MessageTemplate CopyMessageTemplate(MessageTemplate messageTemplate)
		{
			if (messageTemplate == null)
				throw new ArgumentNullException("messageTemplate");

			var mtCopy = new MessageTemplate()
			{
				Name = messageTemplate.Name,
				BccEmailAddresses = messageTemplate.BccEmailAddresses,
				Subject = messageTemplate.Subject,
				Body = messageTemplate.Body,
				IsActive = messageTemplate.IsActive,
				EmailAccountId = messageTemplate.EmailAccountId,
			};

			InsertMessageTemplate(mtCopy);

			var languages = _languageService.GetAllLanguages(true);

			//localization
			foreach (var lang in languages)
			{
				var bccEmailAddresses = messageTemplate.GetLocalized(x => x.BccEmailAddresses, lang.Id, false, false);
				if (!String.IsNullOrEmpty(bccEmailAddresses))
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.BccEmailAddresses, bccEmailAddresses, lang.Id);

				var subject = messageTemplate.GetLocalized(x => x.Subject, lang.Id, false, false);
				if (!String.IsNullOrEmpty(subject))
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.Subject, subject, lang.Id);

				var body = messageTemplate.GetLocalized(x => x.Body, lang.Id, false, false);
				if (!String.IsNullOrEmpty(body))
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.Body, subject, lang.Id);

				var emailAccountId = messageTemplate.GetLocalized(x => x.EmailAccountId, lang.Id, false, false);
				if (emailAccountId > 0)
					_localizedEntityService.SaveLocalizedValue(mtCopy, x => x.EmailAccountId, emailAccountId, lang.Id);
			}

			return mtCopy;
        }
        #endregion Methods
    }
}
