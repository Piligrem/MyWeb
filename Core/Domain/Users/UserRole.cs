using InSearch.Core.Domain.Security;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace InSearch.Core.Domain.Users
{
    [DataContract]
    public partial class UserRole : BaseEntity
    {
        #region Fields
        private ICollection<PermissionRecord> _permissionRecords;
        #endregion Fields

        #region Constructors
        public UserRole()  
        {
            this.CreatedOnUtc = DateTime.UtcNow;
        } 
        #endregion Constructors

        #region Properties
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        [DataMember]
        public string Name { get; set; }

        public bool Active { get; set; }
        public bool IsSystemRole { get; set; }
        public string SystemName { get; set; }
        #endregion Properties

        #region Entity
        public virtual ICollection<PermissionRecord> PermissionRecords
        {
            get { return _permissionRecords ?? (_permissionRecords = new List<PermissionRecord>()); }
            protected set { _permissionRecords = value; }
        }
        #endregion Entity
    }
}
