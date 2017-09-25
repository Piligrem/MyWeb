using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;
using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Common;

namespace InSearch.Data.Mapping.Users
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            this.ToTable("User");
            this.HasKey(c => c.Id);
            this.Property(u => u.CreatedOnUtc).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            this.Property(u => u.Username).HasMaxLength(250);
            this.Property(u => u.Email).HasMaxLength(250);

            this.Ignore(u => u.PasswordFormat);

            // Role - Many to Many
            this.HasMany<UserRole>(u => u.UserRoles)
                .WithMany()
                .Map(m => m.ToTable("UserRole_Mapping"));

            this.HasMany<Address>(c => c.Addresses)
                .WithMany()
                .Map(m => 
                {
                    m.ToTable("UserAddresses_Mapping");
                });


        }
    }
}