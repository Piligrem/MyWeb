using System.Runtime.Serialization;
using InSearch.Core.Configuration;
using InSearch.Core.Domain.Directory;

namespace InSearch.Core.Domain.Common
{
    public partial class SiteSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the site name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the site URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled
        /// </summary>
        public bool SslEnabled { get; set; }

        /// <summary>
        /// Gets or sets the site secure URL (HTTPS)
        /// </summary>
        public string SecureUrl { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of possible HTTP_HOST values
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// Gets or sets the logo picture id
        /// </summary>
        public int LogoPictureId { get; set; }

        public int LogoWidePictureId { get; set; }

        /// <summary>
        /// Gets or sets a store specific id for the HTML body
        /// </summary>
        public string HtmlBodyId { get; set; }

        /// <summary>
        /// Gets or sets the CDN host name, if static media content should be served through a CDN.
        /// </summary>
        public string ContentDeliveryNetwork { get; set; }

        public int PrimaryStoreCurrencyId { get; set; }
        public virtual Currency PrimaryCurrency { get; set; }

        public int PrimaryExchangeRateCurrencyId { get; set; }
        public virtual Currency PrimaryExchangeRateCurrency { get; set; }

        ///// <summary>
        ///// Gets the security mode for the store
        ///// </summary>
        //public HttpSecurityMode GetSecurityMode(bool? useSsl = null)
        //{
        //    if (useSsl ?? SslEnabled)
        //    {
        //        if (SecureUrl.HasValue() && Url.HasValue() && !Url.StartsWith("https"))
        //        {
        //            return HttpSecurityMode.SharedSsl;
        //        }
        //        else
        //        {
        //            return HttpSecurityMode.Ssl;
        //        }
        //    }
        //    return HttpSecurityMode.Unsecured;
        //}
    }
}
