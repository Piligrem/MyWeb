using InSearch.Core.Configuration;

namespace InSearch.Core.Domain.Common
{
    public class SiteInformationSettings : ISettings
    {
        public SiteInformationSettings()
        {
            SiteClosedAllowForAdmins = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether store is closed
        /// </summary>
        public bool SiteClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether administrators can visit a closed store
        /// </summary>
        public bool SiteClosedAllowForAdmins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mini profiler should be displayed in public store (used for debugging)
        /// </summary>
        public bool DisplayMiniProfilerInPublicSite { get; set; }
    }
}
