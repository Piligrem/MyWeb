using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Users;

namespace InSearch.Data.Mapping.Users
{
    public partial class ExternalAuthenticationRecordMap : EntityTypeConfiguration<ExternalAuthenticationRecord>
    {
        public ExternalAuthenticationRecordMap()
        {
            this.ToTable("ExternalAuthenticationRecord");

            this.HasKey(ear => ear.Id);

            this.HasRequired(ear => ear.User)
                .WithMany(c => c.ExternalAuthenticationRecords)
                .HasForeignKey(ear => ear.UserId);
        }
    }
}