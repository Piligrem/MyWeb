using System.Collections.Generic;
using System.Runtime.Serialization;

namespace InSearch.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture
    /// </summary>
	[DataContract]
	public partial class Picture : BaseEntity
    {
        //private ICollection<ProductPicture> _productPictures;
        /// <summary>
        /// Gets or sets the picture binary
        /// </summary>
        public byte[] PictureBinary { get; set; }

        /// <summary>
        /// Gets or sets the picture mime type
        /// </summary>
		[DataMember]
		public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the SEO friendly FileName of the picture
        /// </summary>
		[DataMember]
		public string SeoFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the picture is new
        /// </summary>
		[DataMember]
		public bool IsNew { get; set; }

        [DataMember]
        public bool IsSystem { get; set; }

        ///// <summary>
        ///// Gets or sets the product pictures
        ///// </summary>
        //[DataMember]
        //public virtual ICollection<ProductPicture> ProductPictures
        //{
        //    get { return _productPictures ?? (_productPictures = new List<ProductPicture>()); }
        //    protected set { _productPictures = value; }
        //}
    }
}
