using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Messages;

namespace InSearch.Data.Mapping.Messages
{
    public partial class QueuedNotifyMap : EntityTypeConfiguration<QueuedNotify>
    {
        public QueuedNotifyMap()
        {
            this.ToTable("QueuedNotify");
            this.HasKey(qe => qe.Id);

            this.Property(qe => qe.From).IsRequired().HasMaxLength(500);
            this.Property(qe => qe.FromName).HasMaxLength(500);
            this.Property(qe => qe.To).IsRequired().HasMaxLength(500);
            this.Property(qe => qe.ToName).HasMaxLength(500);
            this.Property(qe => qe.Subject).HasMaxLength(1000);
            this.Property(qe => qe.Body).IsMaxLength();
        }
    }
}