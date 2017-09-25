using System;
using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Security;

namespace InSearch.Data.Mapping.Security
{
    public class PoliciesMap : EntityTypeConfiguration<Policies>
    {
        public PoliciesMap()
        {
            this.ToTable("Policies");
            this.HasKey(c => c.Id);
        }
    }
}