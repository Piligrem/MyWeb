using System;
using InSearch.Core.Configuration;

namespace InSearch.Core.Domain.Common
{
    public class DateTimeSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a default store time zone identifier
        /// </summary>
        public string DefaultSiteTimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether users are allowed to select their time zone
        /// </summary>
        public bool AllowUsersToSetTimeZone { get; set; }

        public static DateTime DefaultDateTime { get { return DateTime.Parse("01.01.1970"); } }
    }
}
