using System;
using System.Collections.Generic;
using System.Linq;
using InSearch.Core.Domain.Localization;

namespace InSearch.Core.Domain.Topics
{
    /// <summary>
    /// Represents a topic
    /// </summary>
	public partial class Topic : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic should be included in sitemap
        /// </summary>
        public bool IncludeInSitemap { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this topic is password protected
        /// </summary>
        public bool IsPasswordProtected { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the topic should also be rendered as a generic html widget
        /// </summary>
        public bool RenderAsWidget { get; set; }

        /// <summary>
        /// Gets or sets the widget zone name
        /// </summary>
        public string WidgetZone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the title should be displayed in the widget block
        /// </summary>
        public bool WidgetShowTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the widget block should have borders
        /// </summary>
        public bool WidgetBordered { get; set; }

        /// <summary>
        /// Gets or sets the sort order (relevant for widgets)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Helper function which gets the comma-separated <c>WidgetZone</c> property as list of strings
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetWidgetZones()
        {
            if (this.WidgetZone.IsEmpty())
            {
                return Enumerable.Empty<string>();
            }
            
            return this.WidgetZone.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        }
    }
}
