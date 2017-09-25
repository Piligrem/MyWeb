using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using EFCache;
using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Directory;
using InSearch.Core.Domain.Localization;
using InSearch.Core.Domain.Logging;
using InSearch.Core.Domain.Messages;
//using InSearch.Core.Domain.Orders;
//using InSearch.Core.Domain.Payments;
using InSearch.Core.Domain.Security;
//using InSearch.Core.Domain.Themes;
using InSearch.Core.Domain.Topics;

namespace InSearch.Data.Caching
{

    /* TODO: (mc)
	 * ========================
	 *		1. Let developers register custom caching policies for single entities (from plugins)
	 *		2. Caching policies should contain expiration info and cacheable rows count
	 *		3. Backend: Let users decide which entities to cache
	 *		4. Backend: Let users purge the cache
	 */

    internal class EfCachingPolicy : CachingPolicy
    {
        private static readonly HashSet<string> _cacheableSets = new HashSet<string>
            {
                typeof(AclRecord).Name,
                typeof(ActivityLogType).Name,
                //typeof(CheckoutAttribute).Name,
                //typeof(CheckoutAttributeValue).Name,
				typeof(Country).Name,
                typeof(Currency).Name,
                typeof(UserRole).Name,
                typeof(EmailAccount).Name,
                typeof(Language).Name,
                typeof(MessageTemplate).Name,
                //typeof(PaymentMethod).Name,
				typeof(PermissionRecord).Name,
				//typeof(ThemeVariable).Name,
				typeof(Topic).Name
            };

        protected override bool CanBeCached(ReadOnlyCollection<EntitySetBase> affectedEntitySets, string sql, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var entitySets = affectedEntitySets.Select(x => x.Name);
            var result = entitySets.All(x => _cacheableSets.Contains(x));
            return result;
        }

        protected override void GetExpirationTimeout(ReadOnlyCollection<EntitySetBase> affectedEntitySets, out TimeSpan slidingExpiration, out DateTimeOffset absoluteExpiration)
        {
            base.GetExpirationTimeout(affectedEntitySets, out slidingExpiration, out absoluteExpiration);
            absoluteExpiration = DateTimeOffset.Now.AddHours(24);
        }

        protected override void GetCacheableRows(ReadOnlyCollection<EntitySetBase> affectedEntitySets, out int minCacheableRows, out int maxCacheableRows)
        {
            base.GetCacheableRows(affectedEntitySets, out minCacheableRows, out maxCacheableRows);
        }
    }
}
