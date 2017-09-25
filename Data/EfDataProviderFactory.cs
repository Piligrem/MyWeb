using System;
using InSearch.Core;
using InSearch.Core.Data;

namespace InSearch.Data
{
    public partial class EfDataProviderFactory : DataProviderFactory
    {
        public EfDataProviderFactory()
            : this(DataSettings.Current)
        {
        }

        public EfDataProviderFactory(DataSettings settings)
            : base(settings)
        {
        }

        public override IDataProvider LoadDataProvider()
        {
            var providerName = Settings.DataProvider;
            if (providerName.IsEmpty())
            {
                throw new InSearchException("Data Settings doesn't contain a providerName");
            }

            switch (providerName.ToLowerInvariant())
            {
                case "sqlserver":
                    return new SqlServerDataProvider();
                case "sqlce":
                    return new SqlCeDataProvider();
                default:
                    throw new InSearchException(string.Format("Unsupported dataprovider name: {0}", providerName));
            }
        }

    }
}
