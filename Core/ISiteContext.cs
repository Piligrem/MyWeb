using InSearch.Core.Domain.Common;

namespace InSearch.Core
{
    public interface ISiteContext
    {
        /// <summary>
        /// Gets or sets the current site
        /// </summary>
        SiteSettings CurrentSite { get; }
    }
}
