namespace InSearch.Core.Data.Hooks
{
    public class HookedEntityEntry
    {
        public object Entity { get; set; }
        public EntityState PreSaveState { get; set; }
    }
}
