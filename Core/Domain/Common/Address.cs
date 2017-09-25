﻿using System;
using System.Runtime.Serialization;
using InSearch.Core.Domain.Directory;
using InSearch.Core.Domain.Users;
using System.Collections.Generic;

namespace InSearch.Core.Domain.Common
{
	[DataContract]
	public class Address : BaseEntity, ICloneable
    {
        #region Fields
        //private ICollection<User> users;
        #endregion Fields

        #region Constructor
        public Address() { }
        #endregion Constructor

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
		[DataMember]
		public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
		[DataMember]
		public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
		[DataMember]
		public string Email { get; set; }

        /// <summary>
        /// Gets or sets the company
        /// </summary>
		[DataMember]
		public string Company { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
		[DataMember]
		public int? CountryId { get; set; }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
		[DataMember]
		public string City { get; set; }

        /// <summary>
        /// Gets or sets the address 1
        /// </summary>
		[DataMember]
		public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address 2
        /// </summary>
		[DataMember]
		public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the zip/postal code
        /// </summary>
		[DataMember]
		public string ZipPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
		[DataMember]
		public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the fax number
        /// </summary>
		[DataMember]
		public string FaxNumber { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the country
        /// </summary>
		[DataMember]
		public virtual Country Country { get; set; }

        public object Clone()
        {
            var addr = new Address()
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Company = this.Company,
                Country = this.Country,
                CountryId = this.CountryId,
                City = this.City,
                Address1 = this.Address1,
                Address2 = this.Address2,
                ZipPostalCode = this.ZipPostalCode,
                PhoneNumber = this.PhoneNumber,
                FaxNumber = this.FaxNumber,
                CreatedOnUtc = this.CreatedOnUtc,
            };
            return addr;
        }

        #region Entity
        //public User User { get; set; }
        //public virtual ICollection<User> Users
        //{
        //    get { return users ?? (users = new List<User>()); }
        //    protected set { users = value; }
        //}
        #endregion Entity
    }
}
