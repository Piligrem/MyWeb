﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSearch.Core.Domain.Localization
{
    public partial class LocalizedProperty : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the locale key group
        /// </summary>
        public string LocaleKeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the locale key
        /// </summary>
        public string LocaleKey { get; set; }

        /// <summary>
        /// Gets or sets the locale value
        /// </summary>
        public string LocaleValue { get; set; }

        /// <summary>
        /// Gets the language
        /// </summary>
        public virtual Language Language { get; set; }
    }
}
