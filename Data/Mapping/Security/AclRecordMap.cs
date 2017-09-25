using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Security;

namespace InSearch.Data.Mapping.Seo
{
    public partial class AclRecordMap : EntityTypeConfiguration<AclRecord>
    {
        public AclRecordMap()
        {
            this.ToTable("AclRecord");
            this.HasKey(lp => lp.Id);

            this.Property(lp => lp.EntityName).IsRequired().HasMaxLength(400);
			this.Property(x => x.IsIdle).IsRequired();
        }
    }
}