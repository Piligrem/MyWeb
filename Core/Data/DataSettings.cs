﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InSearch.Utilities;

namespace InSearch.Core.Data
{
    public partial class DataSettings
    {
        private static readonly ReaderWriterLockSlim s_rwLock = new ReaderWriterLockSlim();
        private static DataSettings s_current = null;
        private static Func<DataSettings> s_settingsFactory = new Func<DataSettings>(() => new DataSettings());
        private static bool? s_installed = null;
        private static bool s_TestMode = false;

        protected const char SEPARATOR = ':';
        protected const string FILENAME = "Settings.txt";

        private DataSettings()
        {
            RawDataSettings = new Dictionary<string, string>();
        }

        #region Static members

        public static void SetDefaultFactory(Func<DataSettings> factory)
        {
            Guard.ArgumentNotNull(() => factory);

            lock (s_rwLock.GetWriteLock())
            {
                s_settingsFactory = factory;
            }
        }

        public static DataSettings Current
        {
            get
            {
                using (s_rwLock.GetUpgradeableReadLock())
                {
                    if (s_current == null)
                    {
                        using (s_rwLock.GetWriteLock())
                        {
                            if (s_current == null)
                            {
                                s_current = s_settingsFactory();
                                s_current.Load();
                            }
                        }
                    }
                }

                return s_current;
            }
        }

        public static bool DatabaseIsInstalled()
        {
            if (s_TestMode)
                return false;

            if (!s_installed.HasValue)
            {
                s_installed = Current.IsValid();
            }

            return s_installed.Value;
        }

        internal static void SetTestMode(bool isTestMode)
        {
            s_TestMode = isTestMode;
        }

        public static void Reload()
        {
            using (s_rwLock.GetWriteLock())
            {
                s_current = null;
                s_installed = null;
            }
        }

        public static void Delete()
        {
            using (s_rwLock.GetWriteLock())
            {
                string filePath = Path.Combine(CommonHelper.MapPath("~/App_Data/"), FILENAME);
                File.Delete(filePath);
                s_current = null;
                s_installed = null;
            }
        }

        #endregion

        #region Instance members

        public string DataProvider { get; set; }

        public string ProviderInvariantName
        {
            get
            {
                if (this.DataProvider.HasValue() && this.DataProvider.IsCaseInsensitiveEqual("sqlserver"))
                    return "System.Data.SqlClient";

                // SqlCe should always be the default provider
                return "System.Data.SqlServerCe.4.0";
            }
        }

        public bool IsSqlServer
        {
            get
            {
                return this.DataProvider.HasValue() && this.DataProvider.IsCaseInsensitiveEqual("sqlserver");
            }
        }

        public string DataConnectionString { get; set; }

        public string DataConnectionStringNotify { get; set; }

        public IDictionary<string, string> RawDataSettings { get; private set; }

        public bool IsValid()
        {
            return this.DataProvider.HasValue() && this.DataConnectionString.HasValue();
        }

        public virtual bool Load()
        {
            using (s_rwLock.GetWriteLock())
            {
                string filePath = Path.Combine(CommonHelper.MapPath("~/App_Data/"), FILENAME);

                this.Reset();

                if (File.Exists(filePath))
                {
                    string text = File.ReadAllText(filePath);
                    var settings = ParseSettings(text);
                    if (settings.Any())
                    {
                        this.RawDataSettings.AddRange(settings);
                        if (settings.ContainsKey("DataProvider"))
                        {
                            this.DataProvider = settings["DataProvider"];
                        }
                        if (settings.ContainsKey("DataConnectionString"))
                        {
                            this.DataConnectionString = settings["DataConnectionString"];
                        }
                        if (settings.ContainsKey("DataConnectionStringNotify"))
                        {
                            this.DataConnectionStringNotify = settings["DataConnectionStringNotify"];
                        }
                        return this.IsValid();
                    }
                }

                return false;
            }
        }

        public void Reset()
        {
            using (s_rwLock.GetWriteLock())
            {
                this.RawDataSettings.Clear();
                this.DataProvider = null;
                this.DataConnectionString = null;
                s_installed = null;
            }
        }

        public virtual bool Save()
        {
            if (!this.IsValid())
                return false;

            using (s_rwLock.GetWriteLock())
            {
                string filePath = Path.Combine(CommonHelper.MapPath("~/App_Data/"), FILENAME);
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath))
                    {
                        // we use 'using' to close the file after it's created
                    }
                }

                var text = SerializeSettings();
                File.WriteAllText(filePath, text);

                return true;
            }
        }

        #endregion

        #region Instance helpers

        protected virtual IDictionary<string, string> ParseSettings(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (text.IsEmpty())
                return result;

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                    settings.Add(str);
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(SEPARATOR);
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                if (key.HasValue() && value.HasValue())
                {
                    result.Add(key, value);
                }
            }

            return result;
        }

        protected virtual string SerializeSettings()
        {
            return string.Format("DataProvider: {0}{2}DataConnectionString: {1}{2}",
                                 this.DataProvider,
                                 this.DataConnectionString,
                                 Environment.NewLine
                );
        }

        #endregion
    }
}