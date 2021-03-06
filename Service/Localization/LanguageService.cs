﻿using System;
using System.Collections.Generic;
using System.Linq;
using InSearch.Core.Caching;
using InSearch.Core.Data;
using InSearch.Core.Domain.Localization;
using InSearch.Core.Events;
using InSearch.Services.Configuration;
//using PVServer.Core.Domain.Stores;
//using PVServer.Services.Stores;

namespace InSearch.Services.Localization
{
    public partial class LanguageService : ILanguageService
    {
        #region Constants
        private const string LANGUAGES_ALL_KEY = "InSearch.language.all-{0}";
        private const string LANGUAGES_COUNT = "InSearch.language.count-{0}";
        private const string LANGUAGES_BY_CULTURE_KEY = "InSearch.language.culture-{0}";
        private const string LANGUAGES_BY_SEOCODE_KEY = "InSearch.language.seocode-{0}";
        private const string LANGUAGES_PATTERN_KEY = "InSearch.language.";
        private const string LANGUAGES_BY_ID_KEY = "InSearch.language.id-{0}";
        #endregion Constants

        #region Fields
        private readonly IRepository<Language> _languageRepository;
        //private readonly IStoreMappingService _storeMappingService;
        private readonly ICacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="languageRepository">Language repository</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="eventPublisher">Event published</param>
        public LanguageService(
            IRepository<Language> languageRepository,
            ISettingService settingService,
            LocalizationSettings localizationSettings,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager
            )
        {
            this._cacheManager = cacheManager;
            this._languageRepository = languageRepository;
            this._settingService = settingService;
            this._localizationSettings = localizationSettings;
            this._eventPublisher = eventPublisher;
            //this._storeMappingService = storeMappingService;
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Deletes a language
        /// </summary>
        /// <param name="language">Language</param>
        public virtual void DeleteLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            //update default admin area language (if required)
            if (_localizationSettings.DefaultAdminLanguageId == language.Id)
            {
                foreach (var activeLanguage in GetAllLanguages())
                {
                    if (activeLanguage.Id != language.Id)
                    {
                        _localizationSettings.DefaultAdminLanguageId = activeLanguage.Id;
                        _settingService.SaveSetting(_localizationSettings);
                        break;
                    }
                }
            }

            _languageRepository.Delete(language);

            //cache
            _cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(language);
        }

        /// <summary>
        /// Gets all languages
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Language collection</returns>
        public virtual IList<Language> GetAllLanguages(bool showHidden = false)
        {
            string key = string.Format(LANGUAGES_ALL_KEY, showHidden);
            var languages = _cacheManager.Get(key, () =>
            {
                var query = _languageRepository.Table;
                if (!showHidden)
                    query = query.Where(l => l.Published);
                query = query.OrderBy(l => l.DisplayOrder);
                return query.ToList();
            });

            languages = languages.ToList();
            return languages;
        }

        /// <summary>
        /// Gets languages count
        /// </summary>
        /// <param name="showHidden">A value indicating whether to consider hidden records</param>
        /// <returns>The count of Languages</returns>
        public virtual int GetLanguagesCount(bool showHidden = false)
        {
            string key = string.Format(LANGUAGES_COUNT, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = _languageRepository.Table;
                if (!showHidden)
                    query = query.Where(l => l.Published);
                return query.Select(x => x.Id).Count();
            });
        }

        /// <summary>
        /// Gets a language
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Language</returns>
        public virtual Language GetLanguageById(int languageId)
        {
            if (languageId == 0)
                return null;

            string key = string.Format(LANGUAGES_BY_ID_KEY, languageId);
            return _cacheManager.Get(key, () =>
            {
                return _languageRepository.GetById(languageId);
            });
        }

        /// <summary>
        /// Gets a language by culture code (e.g.: en-US)
        /// </summary>
        /// <param name="culture">Culture code</param>
        /// <returns>Language</returns>
        public virtual Language GetLanguageByCulture(string culture)
        {
            if (!culture.HasValue())
                return null;

            string key = string.Format(LANGUAGES_BY_CULTURE_KEY, culture);
            return _cacheManager.Get(key, () =>
            {
                return _languageRepository.Table.Where(x => culture.Equals(x.LanguageCulture, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            });
        }

        public virtual Language GetLanguageBySeoCode(string seoCode)
        {
            if (!seoCode.HasValue())
                return null;

            string key = string.Format(LANGUAGES_BY_SEOCODE_KEY, seoCode);
            return _cacheManager.Get(key, () =>
            {
                return _languageRepository.Table.Where(x => seoCode.Equals(x.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            });
        }

        /// <summary>
        /// Inserts a language
        /// </summary>
        /// <param name="language">Language</param>
        public virtual void InsertLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            _languageRepository.Insert(language);

            //cache
            _cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(language);
        }

        /// <summary>
        /// Updates a language
        /// </summary>
        /// <param name="language">Language</param>
        public virtual void UpdateLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            //update language
            _languageRepository.Update(language);

            //cache
            _cacheManager.RemoveByPattern(LANGUAGES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(language);
        }
        #endregion Methods
    }
}
