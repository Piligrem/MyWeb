using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

using InSearch.Core.Caching;
using InSearch.Core.Configuration;
using InSearch.Core.Data;
using InSearch.Core.Domain.Configuration;
using InSearch.Core.Events;
using InSearch.Core.ComponentModel;
using InSearch.ComponentModel;

namespace InSearch.Services.Configuration
{
    public partial class SettingService : ISettingService
    {
        #region Constants
        private const string SETTINGS_ALL_KEY = "InSearch.setting.all";
        #endregion

        #region Fields
        private readonly IRepository<Setting> _settingRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="settingRepository">Setting repository</param>
        public SettingService(ICacheManager cacheManager, IEventPublisher eventPublisher, IRepository<Setting> settingRepository)
        {
            this._cacheManager = cacheManager;
            this._eventPublisher = eventPublisher;
            this._settingRepository = settingRepository;
        }
        #endregion

        #region Nested classes
        [Serializable]
        public class SettingForCaching
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region Utilities
        /// <summary>
        /// Gets all settings
        /// </summary>
        /// <returns>Setting collection</returns>
        protected virtual IDictionary<string, IList<SettingForCaching>> GetAllSettingsCached()
        {
            //cache
            string key = string.Format(SETTINGS_ALL_KEY);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in _settingRepository.TableUntracked
                            orderby s.Name
                            select s;
                var settings = query.ToList();
                var dictionary = new Dictionary<string, IList<SettingForCaching>>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in settings)
                {
                    var settingName = s.Name.ToLowerInvariant();
                    var settingForCaching = new SettingForCaching()
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Value = s.Value,
                    };
                    if (!dictionary.ContainsKey(settingName))
                    {
                        //first setting
                        dictionary.Add(settingName, new List<SettingForCaching>()
                        {
                            settingForCaching
                        });
                    }
                    else
                    {
                        dictionary[settingName].Add(settingForCaching);
                    }
                }
                return dictionary;
            });
        }

        /// <summary>
        /// Adds a setting
        /// </summary>
        /// <param name="setting">Setting</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        public virtual void InsertSetting(Setting setting, bool clearCache = true)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            _settingRepository.Insert(setting);

            //cache
            if (clearCache)
                _cacheManager.RemoveByPattern(SETTINGS_ALL_KEY);

            //event notification
            _eventPublisher.EntityInserted(setting);
        }

        /// <summary>
        /// Updates a setting
        /// </summary>
        /// <param name="setting">Setting</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        public virtual void UpdateSetting(Setting setting, bool clearCache = true)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            _settingRepository.Update(setting);

            //cache
            if (clearCache)
                _cacheManager.RemoveByPattern(SETTINGS_ALL_KEY);

            //event notification
            _eventPublisher.EntityUpdated(setting);
        }

        private T LoadSettingsJson<T>()
        {
            Type t = typeof(T);
            string key = t.Namespace + "." + t.Name;

            T settings = Activator.CreateInstance<T>();

            var rawSetting = GetSettingByKey<string>(key);
            if (rawSetting.HasValue())
            {
                JsonConvert.PopulateObject(rawSetting, settings);
            }

            return settings;
        }

        private void SaveSettingsJson<T>(T settings)
        {
            Type t = typeof(T);
            string key = t.Namespace + "." + t.Name;

            var rawSettings = JsonConvert.SerializeObject(settings);
            SetSetting(key, rawSettings, false);

            // and now clear cache
            ClearCache();
        }

        private void DeleteSettingsJson<T>()
        {
            Type t = typeof(T);
            string key = t.Namespace + "." + t.Name;

            var setting = GetAllSettings()
                .FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (setting != null)
            {
                DeleteSetting(setting);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a setting by identifier
        /// </summary>
        /// <param name="settingId">Setting identifier</param>
        /// <returns>Setting</returns>
        public virtual Setting GetSettingById(int settingId)
        {
            if (settingId == 0)
                return null;

            var setting = _settingRepository.GetById(settingId);
            return setting;
        }

        /// <summary>
        /// Get setting value by key
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Setting value</returns>
		public virtual T GetSettingByKey<T>(string key, T defaultValue = default(T))
        {
            if (String.IsNullOrEmpty(key))
                return defaultValue;

            var settings = GetAllSettingsCached();
            key = key.Trim().ToLowerInvariant();
            if (settings.ContainsKey(key))
            {
                var settingsByKey = settings[key];
                var setting = settingsByKey.FirstOrDefault();

                // load shared value?
                if (setting == null)
                    setting = settingsByKey.FirstOrDefault();

                if (setting != null)
                    return setting.Value.Convert<T>();
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets all settings
        /// </summary>
        /// <returns>Setting collection</returns>
        public virtual IList<Setting> GetAllSettings()
        {
            var query = from s in _settingRepository.Table
                        orderby s.Name
                        select s;
            var settings = query.ToList();
            return settings;
        }

        /// <summary>
        /// Determines whether a setting exists
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>true -setting exists; false - does not exist</returns>
        public virtual bool SettingExists<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector)
            where T : ISettings, new()
        {
            var member = keySelector.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    keySelector));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(
                       "Expression '{0}' refers to a field, not a property.",
                       keySelector));
            }

            string key = typeof(T).Name + "." + propInfo.Name;

            string setting = GetSettingByKey<string>(key);
            return setting != null;
        }

        /// Load settings
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public virtual T LoadSetting<T>() where T : ISettings, new()
        {
            if (typeof(T).HasAttribute<JsonPersistAttribute>(true))
            {
                return LoadSettingsJson<T>();
            }

            var settings = Activator.CreateInstance<T>();

            foreach (var fastProp in FastProperty.GetProperties(typeof(T)).Values)
            {
                var prop = fastProp.Property;

                // get properties we can read and write to
                if (!prop.CanWrite)
                    continue;

                var key = typeof(T).Name + "." + prop.Name;

                string setting = GetSettingByKey<string>(key);

                if (setting == null && !fastProp.IsSequenceType)
                {
                    #region Obsolete ('EnumerableConverter' can handle this case now)
                    //if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    //{
                    //	// convenience: don't return null for simple list types
                    //	var listArg = prop.PropertyType.GetGenericArguments()[0];
                    //	object list = null;

                    //	if (listArg == typeof(int))
                    //		list = new List<int>();
                    //	else if (listArg == typeof(decimal))
                    //		list = new List<decimal>();
                    //	else if (listArg == typeof(string))
                    //		list = new List<string>();

                    //	if (list != null)
                    //	{
                    //		fastProp.SetValue(settings, list);
                    //	}
                    //}
                    #endregion

                    continue;
                }

                var converter = TypeConverterFactory.GetConverter(prop.PropertyType);

                if (converter == null || !converter.CanConvertFrom(typeof(string)))
                    continue;

                object value = converter.ConvertFrom(setting);

                //set property
                fastProp.SetValue(settings, value);
            }

            return settings;
        }

        /// <summary>
        /// Set setting value
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
		public virtual void SetSetting<T>(string key, T value, bool clearCache = true)
        {
            Guard.ArgumentNotEmpty(() => key);

            key = key.Trim().ToLowerInvariant();
            var str = value.Convert<string>();

            var allSettings = GetAllSettingsCached();
            var settingForCaching = allSettings.ContainsKey(key) ?
                allSettings[key].FirstOrDefault() : null;

            if (settingForCaching != null)
            {
                //update
                var setting = GetSettingById(settingForCaching.Id);
                setting.Value = str;
                UpdateSetting(setting, clearCache);
            }
            else
            {
                //insert
                var setting = new Setting
                {
                    Name = key,
                    Value = str,
                };
                InsertSetting(setting, clearCache);
            }
        }

        /// <summary>
        /// Save settings object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
		/// <param name="settings">Setting instance</param>
		public virtual void SaveSetting<T>(T settings) where T : ISettings, new()
        {
            if (typeof(T).HasAttribute<JsonPersistAttribute>(true))
            {
                SaveSettingsJson<T>(settings);
                return;
            }

            /* We do not clear cache after each setting update.
			 * This behavior can increase performance because cached settings will not be cleared 
			 * and loaded from database after each update */
            foreach (var prop in FastProperty.GetProperties(typeof(T)).Values)
            {
                // get properties we can read and write to
                if (!prop.IsPublicSettable)
                    continue;

                var converter = TypeConverterFactory.GetConverter(prop.Property.PropertyType);
                if (converter == null || !converter.CanConvertFrom(typeof(string)))
                    continue;

                string key = typeof(T).Name + "." + prop.Name;
                // Duck typing is not supported in C#. That's why we're using dynamic type
                dynamic value = prop.GetValue(settings);

                SetSetting(key, value ?? "", false);
            }

            //and now clear cache
            ClearCache();
        }

        /// Save settings object
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        public virtual void SaveSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector, bool clearCache = true) where T : ISettings, new()
        {
            var member = keySelector.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    keySelector));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(
                       "Expression '{0}' refers to a field, not a property.",
                       keySelector));
            }

            string key = typeof(T).Name + "." + propInfo.Name;
            // Duck typing is not supported in C#. That's why we're using dynamic type
            var fastProp = FastProperty.GetProperty(propInfo, PropertyCachingStrategy.EagerCached);
            dynamic value = fastProp.GetValue(settings);

            SetSetting(key, value ?? "", false);
        }

        public virtual void UpdateSetting<T, TPropType>(T settings, Expression<Func<T, TPropType>> keySelector) where T : ISettings, new()
        {
            SaveSetting(settings, keySelector, false);
        }

        /// <summary>
        /// Deletes a setting
        /// </summary>
        /// <param name="setting">Setting</param>
        public virtual void DeleteSetting(Setting setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            _settingRepository.Delete(setting);

            //cache
            _cacheManager.RemoveByPattern(SETTINGS_ALL_KEY);

            //event notification
            _eventPublisher.EntityDeleted(setting);
        }

        /// <summary>
        /// Delete all settings
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public virtual void DeleteSetting<T>() where T : ISettings, new()
        {
            if (typeof(T).HasAttribute<JsonPersistAttribute>(true))
            {
                DeleteSettingsJson<T>();
                return;
            }

            var settingsToDelete = new List<Setting>();
            var allSettings = GetAllSettings();
            foreach (var prop in typeof(T).GetProperties())
            {
                string key = typeof(T).Name + "." + prop.Name;
                settingsToDelete.AddRange(allSettings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
            }

            foreach (var setting in settingsToDelete)
                DeleteSetting(setting);
        }

        /// Delete settings object
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        public virtual void DeleteSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector) where T : ISettings, new()
        {
            var member = keySelector.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    keySelector));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(
                       "Expression '{0}' refers to a field, not a property.",
                       keySelector));
            }

            string key = typeof(T).Name + "." + propInfo.Name;

            DeleteSetting(key);
        }

        public virtual void DeleteSetting(string key)
        {
            if (key.HasValue())
            {
                key = key.Trim().ToLowerInvariant();

                var setting = (
                    from s in _settingRepository.Table
                    where s.Name == key
                    select s).FirstOrDefault();

                if (setting != null)
                    DeleteSetting(setting);
            }
        }

        /// <summary>
        /// Deletes all settings with its key beginning with rootKey.
        /// </summary>
        /// <returns>Number of deleted settings</returns>
        public virtual int DeleteSettings(string rootKey)
        {
            int result = 0;

            if (rootKey.HasValue())
            {
                try
                {
                    string sqlDelete = "DELETE FROM [Setting] WHERE [Name] LIKE '{0}%'".FormatWith(rootKey.EndsWith(".") ? rootKey : rootKey + ".");
                    result = _settingRepository.Context.ExecuteSqlCommand(sqlDelete);

                    // cache
                    _cacheManager.RemoveByPattern(SETTINGS_ALL_KEY);
                }
                catch (Exception exc)
                {
                    exc.Dump();
                }
            }

            return result;
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public virtual void ClearCache()
        {
            _cacheManager.RemoveByPattern(SETTINGS_ALL_KEY);
        }
        #endregion
    }
}
