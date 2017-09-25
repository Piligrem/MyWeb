using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Forums;

namespace InSearch.Data.Mapping.Forums
{
    public partial class ForumGroupMap : EntityTypeConfiguration<ForumGroup>
    {
        public ForumGroupMap()
        {
            this.ToTable("Forums_Group");
            this.HasKey(fg => fg.Id);
            this.Property(fg => fg.Name).IsRequired().HasMaxLength(200);
			this.Property(fg => fg.Description).IsMaxLength();
        }
    }
}
