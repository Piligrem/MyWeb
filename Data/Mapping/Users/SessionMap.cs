using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;
using InSearch.Core.Domain.Users;

namespace InSearch.Data.Mapping.Users
{
    public class SessionMap : EntityTypeConfiguration<Session>
    {
        public SessionMap()
        {
            this.ToTable("Session");
            this.HasKey(c => c.Id);
            this.Property(u => u.CreatedOnUtc).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
        }
    }
}