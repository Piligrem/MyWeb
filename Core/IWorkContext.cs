using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Localization;

namespace InSearch.Core
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current user
        /// </summary>
        User CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets the original user (in case the current one is impersonated)
        /// </summary>
        User OriginalUserIfImpersonated { get; }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        Language WorkingLanguage { get; set; }

        /// <summary>
        /// Gets a value indicating whether a language exists and is published within a store's scope.
        /// </summary>
        /// <param name="seoCode">The unique seo code of the language to check for</param>
        /// <returns>Whether the language exists and is published</returns>
        bool IsPublishedLanguage(string seoCode);

        /// <summary>
        /// Gets the default (fallback) language for a store
        /// </summary>
        /// <returns>The unique seo code of the language to check for</returns>
        string GetDefaultLanguageSeoCode();

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        //Currency WorkingCurrency { get; set; }

        /// <summary>
        /// Get or set value indicating whether we're in admin area
        /// </summary>
        bool IsAdmin { get; set; }

        ///// <summary>
        ///// Get or set a value indicating whether we're in the public shop
        ///// </summary>
        //bool IsPublic { get; set; }
    }
}
