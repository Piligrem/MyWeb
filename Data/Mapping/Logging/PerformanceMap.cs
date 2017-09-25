using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Logging;

namespace InSearch.Data.Mapping.Logging
{
    public partial class PerformanceMap : EntityTypeConfiguration<Performance>
    {
        public PerformanceMap()
        {
            this.ToTable("Performance");
            this.HasKey(p => p.Id);
        }
    }
}