﻿using System.Collections.Generic;
using InSearch.Core.Domain.Localization;

namespace InSearch.Services.Localization
{
    public partial interface ILanguageService
    {
        /// <summary>
        /// Deletes a language
        /// </summary>
        /// <param name="language">Language</param>
        void DeleteLanguage(Language language);

        /// <summary>
        /// Gets all languages
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Language collection</returns>
        IList<Language> GetAllLanguages(bool showHidden = false);

        /// <summary>
        /// Gets languages count
        /// </summary>
        /// <param name="showHidden">A value indicating whether to consider hidden records</param>
        /// <returns>The count of Languages</returns>
        int GetLanguagesCount(bool showHidden = false);

        /// <summary>
        /// Gets a language
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Language</returns>
        Language GetLanguageById(int languageId);

        /// <summary>
        /// Gets a language by culture code (e.g.: en-US)
        /// </summary>
        /// <param name="culture">Culture code</param>
        /// <returns>Language</returns>
        Language GetLanguageByCulture(string culture);

        /// <summary>
        /// Gets a language by it's unique seo code (e.g.: en)
        /// </summary>
        /// <param name="seoCode">SEO code</param>
        /// <returns>Language</returns>
        Language GetLanguageBySeoCode(string seoCode);

        /// <summary>
        /// Inserts a language
        /// </summary>
        /// <param name="language">Language</param>
        void InsertLanguage(Language language);

        /// <summary>
        /// Updates a language
        /// </summary>
        /// <param name="language">Language</param>
        void UpdateLanguage(Language language);
    }
}
