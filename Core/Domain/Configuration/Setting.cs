using System;
using System.Runtime.Serialization;

namespace InSearch.Core.Domain.Configuration
{
    
       [DataContract]
    public partial class Setting : BaseEntity
    {
        #region Fields
        #endregion Fields

        #region Constructors
        #endregion Constructors

        #region Properties
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
        #endregion Properties
    }
}
