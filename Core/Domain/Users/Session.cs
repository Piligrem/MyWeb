using System;
using System.Collections.Generic;
using InSearch.Core.Data;

namespace InSearch.Core.Domain.Users
{
    public partial class Session : BaseEntity
    {
        #region Fields
        #endregion Fields

        #region Constructors
        public Session() { }
        #endregion Constructors

        #region Properties
        public int UserId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? ExpireOnUtc { get; set; }
        public string IPAddress { get; set; }
        #endregion Properties

        #region Navigation properties
        #endregion Navigation properties
    }
}
