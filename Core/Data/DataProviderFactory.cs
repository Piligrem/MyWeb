namespace InSearch.Core.Data
{
    public abstract class DataProviderFactory
    {
        protected DataProviderFactory(DataSettings settings)
        {
            Guard.ArgumentNotNull(() => settings);
            this.Settings = settings;
        }

        protected DataSettings Settings { get; private set; }

        public abstract IDataProvider LoadDataProvider();
    }
}
