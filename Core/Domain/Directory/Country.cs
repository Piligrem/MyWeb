using System.Runtime.Serialization;
using InSearch.Core.Domain.Localization;

namespace InSearch.Core.Domain.Directory
{
    /// <summary>
    /// Represents a country
    /// </summary>
	[DataContract]
	public partial class Country : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the two letter ISO code
        /// </summary>
		[DataMember]
		public string TwoLetterIsoCode { get; set; }

        /// <summary>
        /// Gets or sets the three letter ISO code
        /// </summary>
		[DataMember]
		public string ThreeLetterIsoCode { get; set; }

        /// <summary>
        /// Gets or sets the numeric ISO code
        /// </summary>
		[DataMember]
		public int NumericIsoCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether users in this country must be charged EU VAT
        /// </summary>
		[DataMember]
		public bool SubjectToVat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
		[DataMember]
		public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }
    }
}
