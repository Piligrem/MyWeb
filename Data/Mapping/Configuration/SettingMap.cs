using System;
using System.Data.Entity.ModelConfiguration;
using InSearch.Core.Domain.Configuration;

namespace InSearch.Data.Mapping.Configuration
{
    public class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            this.ToTable("Setting");
            this.HasKey(c => c.Id);
        }
    }
}