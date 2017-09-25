using System;
using System.Runtime.Serialization;

namespace InSearch.Core.Domain.Security
{
    [DataContract]
    public partial class Policies : BaseEntity
    {
        #region Fields
        #endregion Fields

        #region Constructors
        public Policies() 
        {
        }  
        #endregion Constructors

        #region Properties
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        [DataMember]
        public int RequiredLength { get; set; }

        [DataMember]
        public bool RequireNonLetterOrDigit { get; set; }

        [DataMember]
        public bool RequireDigit { get; set; }

        [DataMember]
        public bool RequireLowercase { get; set; }

        [DataMember]
        public bool RequireUppercase { get; set; }
        #endregion Properties

        #region Entity
        #endregion Entity
    }
}
