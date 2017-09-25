using InSearch.Core.Domain.Directory;
using System.Data.Entity.ModelConfiguration;

namespace InSearch.Data.Mapping.Directory
{
    public partial class CurrencyMap : EntityTypeConfiguration<Currency>
    {
        public CurrencyMap()
        {
            this.ToTable("Currency");
            this.HasKey(c => c.Id);

            this.Property(c => c.Name).IsRequired().HasMaxLength(50);
            this.Property(c => c.CurrencyCode).HasMaxLength(5);
            this.Property(c => c.Rate).HasPrecision(18,8);
            this.Property(c => c.DisplayLocale).HasMaxLength(50);
            this.Property(c => c.CustomFormatting).HasMaxLength(50);
            this.Property(c => c.DomainEndings).HasMaxLength(1000);
        }

    }
}
