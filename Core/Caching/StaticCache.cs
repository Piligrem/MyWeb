using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.Caching;
using InSearch.Utilities;
using InSearch.Utilities.Threading;
using System.IO;
using System.Diagnostics;

namespace InSearch.Core.Caching
{
    public partial class StaticCache : ICache
    {
		private ObjectCache _cache;
        // ??? Temporary for debug
        //private CacheEntryRemovedCallback removedCallback = null;
        //private CacheEntryUpdateCallback updateCallback = null;
        // ??? Temporary for debug

        protected ObjectCache Cache
        {
            get
            {
				if (_cache == null)
				{
                    _cache = new MemoryCache("InSearch");
                    // ??? Temporary for debug
                    //removedCallback = new CacheEntryRemovedCallback(this.CachedItemRemovedCallback);
                    //updateCallback = new CacheEntryUpdateCallback(this.CachedItemUpdateCallback);
                    // ??? Temporary for debug
				}
				return _cache;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get { return Cache; }
        }

		public object Get(string key)
        {
			return Cache.Get(key);
        }

		public void Set(string key, object value, int? cacheTime)
		{
			var cacheItem = new CacheItem(key, value);
			CacheItemPolicy policy = null;
            // --- ??? Temporary for debug
            //policy = new CacheItemPolicy();
            //policy.RemovedCallback = removedCallback;
            //policy.UpdateCallback = updateCallback;
            // --- ??? Temporary for debug
			if (cacheTime.GetValueOrDefault() > 0)
			{
				policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTime.Value) };
			}
            Cache.Add(cacheItem, policy);
		}

        public bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

		public bool IsSingleton
		{
			get { return true; }
		}

        // ??? Temporary for debug
        private void CachedItemRemovedCallback(CacheEntryRemovedArguments arguments) 
        { 
            // Log these values from arguments list 
            String strLog = String.Concat("Reason: ", 
                arguments.RemovedReason.ToString(), 
                " | Key-Name: ", 
                arguments.CacheItem.Key, 
                " | Value-Object: ", 
                arguments.CacheItem.Value.ToString());
            Debug.WriteLine(strLog);
        }
        private void CachedItemUpdateCallback(CacheEntryUpdateArguments arguments)
        {
            // Log these values from arguments list 
            String strLog = String.Concat("Reason: ",
                arguments.RemovedReason.ToString(),
                " | Key-Name: ",
                arguments.UpdatedCacheItem.Key,
                " | Value-Object: ",
                arguments.UpdatedCacheItem.Value.ToString());
            Debug.WriteLine(strLog);
        }
        // ??? Temporary for debug
	}
}
