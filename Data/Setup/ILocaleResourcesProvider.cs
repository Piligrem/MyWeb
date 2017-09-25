using System;

namespace InSearch.Data.Setup
{

    public interface ILocaleResourcesProvider
    {
        void MigrateLocaleResources(LocaleResourcesBuilder builder);
    }

}
