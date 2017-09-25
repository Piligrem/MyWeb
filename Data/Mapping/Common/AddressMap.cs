using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Common;

namespace InSearch.Data.Mapping.Common
{
    public partial class AddressMap : EntityTypeConfiguration<Address>
    {
        public AddressMap()
        {
            this.ToTable("Address");
            this.HasKey(a => a.Id);

            //this.HasOptional(a => a.Country)
            //    .WithMany()
            //    .HasForeignKey(a => a.CountryId).WillCascadeOnDelete(false);
        }
    }
}
