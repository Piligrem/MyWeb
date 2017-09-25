using System.Runtime.Serialization;

namespace InSearch.Core.Domain.Common
{
    [DataContract]
    public class Property : BaseEntity
    {
        #region Properties
        /// <summary>
        /// Gets or sets name property
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        public string Description { get; set; }
        #endregion Properties


    }
}
