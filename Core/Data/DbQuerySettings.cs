namespace InSearch.Core.Data
{
    public class DbQuerySettings
    {
        private readonly static DbQuerySettings s_default = new DbQuerySettings(false);

        public DbQuerySettings(bool ignoreAcl)
        {
            this.IgnoreAcl = ignoreAcl;
        }

        public bool IgnoreAcl { get; private set; }

        public static DbQuerySettings Default
        {
            get { return s_default; }
        }
    }
}
