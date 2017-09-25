using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Common;

namespace InSearch.Data.Mapping.Common
{
    public partial class PropertyMap : EntityTypeConfiguration<Property>
    {
        public PropertyMap()
        {
            this.ToTable("Property");
            this.HasKey(a => a.Id);

        }
    }
}
