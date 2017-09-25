using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Topics;

namespace InSearch.Data.Mapping.Topics
{
    public class TopicMap : EntityTypeConfiguration<Topic>
    {
        public TopicMap()
        {
            this.ToTable("Topic");
            this.HasKey(t => t.Id);
			this.Property(t => t.Body).IsMaxLength();
        }
    }
}
