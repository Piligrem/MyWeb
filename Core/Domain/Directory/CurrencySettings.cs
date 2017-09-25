using InSearch.Core.Configuration;

namespace InSearch.Core.Domain.Directory
{
    public class CurrencySettings : ISettings
    {
        public int PrimaryCurrencyId { get; set; }
        public int PrimaryExchangeRateCurrencyId { get; set; }
        public string ActiveExchangeRateProviderSystemName { get; set; }
        public bool AutoUpdateEnabled { get; set; }
        public long LastUpdateTime { get; set; }
    }
}