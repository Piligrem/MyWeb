using System;
using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace InSearch.Data.Mapping.Users
{
    public class UserRoleMap : EntityTypeConfiguration<UserRole>
    {
        public UserRoleMap()
        {
            this.ToTable("UserRole");
            this.HasKey(c => c.Id);

            this.Property(u => u.CreatedOnUtc).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(u => u.Name).HasMaxLength(250).IsRequired();
        }
    }
}