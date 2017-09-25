using InSearch.Core.Caching;
using InSearch.Core.Infrastructure;
using InSearch.Services.Tasks;

namespace InSearch.Services.Caching
{
    /// <summary>
    /// Clear cache scheduled task implementation
    /// </summary>
    public partial class ClearCacheTask : ITask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var cacheManager = EngineContext.Current.Resolve<ICacheManager>("static");
            cacheManager.Clear();
        }
    }
}
