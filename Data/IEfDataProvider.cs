using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using InSearch.Core.Data;

namespace InSearch.Data
{
    public interface IEfDataProvider : IDataProvider
    {
        /// <summary>
        /// Get connection factory
        /// </summary>
        /// <returns>Connection factory</returns>
        IDbConnectionFactory GetConnectionFactory();

    }
}
