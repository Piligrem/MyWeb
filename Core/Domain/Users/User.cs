using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using InSearch.Core.Domain.Common;
using InSearch.Core.Domain.Forums;

namespace InSearch.Core.Domain.Users
{
    [DataContract]
    public partial class User : BaseEntity, ISoftDeletable
    {
        #region Fields
        private ICollection<UserRole> userRoles;
        private ICollection<Address> addresses;
        private ICollection<ExternalAuthenticationRecord> externalAuthenticationRecords;
        //private ICollection<Order> _orders;
        private ICollection<ForumTopic> _forumTopics;
        private ICollection<ForumPost> _forumPosts;

        #endregion Fields

        #region Constructors
        public User() 
        {
            this.PasswordFormat = PasswordFormat.Clear;
            this.UserGuid = Guid.NewGuid();
            this.CreatedOnUtc = DateTime.UtcNow;
        } 
        #endregion Constructors

        #region Properties
        [DataMember]
        public Guid UserGuid { get; set; }
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        public string Password { get; set; }
        public int PasswordFormatId { get; set; }
        [NotMapped]
        public PasswordFormat PasswordFormat
        {
            get { return (PasswordFormat)PasswordFormatId; }
            set { this.PasswordFormatId = (int)value; }
        }
        public string PasswordSalt { get; set; }
        [DataMember]
        public string AdminComment { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public int UserTypeId { get; set; }
        [NotMapped]
        public UserType UserType
        {
            get { return (UserType)UserTypeId; }
            set { UserTypeId = (int)value; }
        }
        [DataMember]
        public bool Active { get; set; }
        [DataMember]
        public bool Deleted { get; set; }
        [DataMember]
        public bool IsSystemAccount { get; set; }
        [DataMember]
        public string SystemName { get; set; }
        [DataMember]
        public DateTime? LastLoginDateUtc { get; set; }
        [DataMember]
        public DateTime? LastActivityDateUtc { get; set; }
        [DataMember]
        public string LastIpAddress { get; set; }
        [DataMember]
        public virtual Address BillingAddress { get; set; }
        #endregion Properties

        #region Addresses
        public virtual void RemoveAddress(Address address)
        {
            if (this.Addresses.Contains(address))
                this.Addresses.Remove(address);
        }
        #endregion Addresses

        #region Navigation properties
        public virtual ICollection<UserRole> UserRoles 
        {
            get { return userRoles ?? (userRoles = new HashSet<UserRole>()); }
            protected set { userRoles = value; }
        }
        
        [DataMember]
        public virtual ICollection<Address> Addresses
        {
            get { return addresses ?? (addresses = new HashSet<Address>()); }
            protected set { addresses = value; }
        }
        public virtual ICollection<ForumTopic> ForumTopics
        {
            get { return _forumTopics ?? (_forumTopics = new HashSet<ForumTopic>()); }
            protected set { _forumTopics = value; }
        }
        public virtual ICollection<ForumPost> ForumPosts
        {
            get { return _forumPosts ?? (_forumPosts = new HashSet<ForumPost>()); }
            protected set { _forumPosts = value; }
        }
        public virtual ICollection<ExternalAuthenticationRecord> ExternalAuthenticationRecords
        {
            get { return externalAuthenticationRecords ?? (externalAuthenticationRecords = new HashSet<ExternalAuthenticationRecord>()); }
            protected set { externalAuthenticationRecords = value; }
        }

        #endregion Navigation properties

    }
}
