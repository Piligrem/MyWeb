using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using InSearch.Core.Configuration;
using InSearch.Core.Infrastructure;
using InSearch.Utilities;

namespace InSearch
{
    public class EngineContext
    {
        #region Initialization Methods
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine Initialize(bool forceRecreate, IEngine engine = null)
        {
            if (Singleton<IEngine>.Instance == null || forceRecreate)
            {
                //var config = ConfigurationManager.GetSection("InSearchConfig") as InSearchConfig;
                Singleton<IEngine>.Instance = engine ?? CreateEngineInstance();
                Singleton<IEngine>.Instance.Initialize();
            }
            return Singleton<IEngine>.Instance;
        }

        /// <summary>Sets the static engine instance to the supplied engine. Use this method to supply your own engine implementation.</summary>
        /// <param name="engine">The engine to use.</param>
        /// <remarks>Only use this method if you know what you're doing.</remarks>
        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }

        /// <summary>
        /// Creates a factory instance and adds a http application injecting facility.
        /// </summary>
        /// <returns>A new factory</returns>
        public static IEngine CreateEngineInstance()
        {
            var engineTypeSetting = CommonHelper.GetAppSetting<string>("InSearch:EngineType");
            if (engineTypeSetting.HasValue())
            {
                var engineType = Type.GetType(engineTypeSetting);
                if (engineType == null)
                    throw new ConfigurationErrorsException("The type '" + engineType + "' could not be found. Please check the configuration at /configuration/InSearchConfig/engine[@engineType] or check for missing assemblies.");
                if (!typeof(IEngine).IsAssignableFrom(engineType))
                    throw new ConfigurationErrorsException("The type '" + engineType + "' doesn't implement 'InSearch.Core.Infrastructure.IEngine' and cannot be configured in /configuration/InSearchConfig/engine[@engineType] for that purpose.");
                return Activator.CreateInstance(engineType) as IEngine;
            }

            return new InSearchEngine();
        }

        public static IEngine CreateEngineInstance(InSearchConfig config)
        {
            if (config != null && !string.IsNullOrEmpty(config.EngineType))
            {
                var engineType = Type.GetType(config.EngineType);
                if (engineType == null)
                    throw new ConfigurationErrorsException("The type '" + engineType + "' could not be found. Please check the configuration at /configuration/InSearchConfig/engine[@engineType] or check for missing assemblies.");
                if (!typeof(IEngine).IsAssignableFrom(engineType))
                    throw new ConfigurationErrorsException("The type '" + engineType + "' doesn't implement 'InSearch.Core.Infrastructure.IEngine' and cannot be configured in /configuration/InSearchConfig/engine[@engineType] for that purpose.");
                return Activator.CreateInstance(engineType) as IEngine;
            }

            return new InSearchEngine();
        }
        #endregion Initialization Methods

        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null)
                {
                    Initialize(false);
                }
                return Singleton<IEngine>.Instance;
            }
        }
    }
}
