using System;
using Autofac;
using InSearch.Core;
using InSearch.Core.Caching;
using InSearch.Core.Data;
using InSearch.Core.Events;
using InSearch.Services.Localization;
using InSearch.Core.Logging;
using InSearch.Services.Security;
using InSearch.Services.Logging;
using InSearch.Services.Helpers;
using InSearch.Services.Configuration;

namespace InSearch.Services
{
    public class CommonServices : ICommonServices
    {
        private readonly IComponentContext _container;
        private readonly Lazy<ICacheManager> _cache;
        private readonly Lazy<IDbContext> _dbContext;
        private readonly Lazy<ISiteContext> _siteContext;
        private readonly Lazy<IWebHelper> _webHelper;
        private readonly Lazy<IWorkContext> _workContext;
        private readonly Lazy<IEventPublisher> _eventPublisher;
        private readonly Lazy<ILocalizationService> _localization;
        private readonly Lazy<IUserActivityService> _userActivity;
        private readonly Lazy<INotifier> _notifier;
        private readonly Lazy<IPermissionService> _permissions;
        private readonly Lazy<ISettingService> _settings;
        private readonly Lazy<IDateTimeHelper> _dateTimeHelper;

        public CommonServices(
            IComponentContext container,
            Func<string, Lazy<ICacheManager>> cache,
            Lazy<IDbContext> dbContext,
            Lazy<ISiteContext> siteContext,
            Lazy<IWebHelper> webHelper,
            Lazy<IWorkContext> workContext,
            Lazy<IEventPublisher> eventPublisher,
            Lazy<ILocalizationService> localization,
            Lazy<IUserActivityService> userActivity,
            Lazy<INotifier> notifier,
            Lazy<IPermissionService> permissions,
            Lazy<ISettingService> settings,
            Lazy<IDateTimeHelper> dateTimeHelper)
        {
            this._container = container;
            this._cache = cache("static");
            this._dbContext = dbContext;
            this._siteContext = siteContext;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._eventPublisher = eventPublisher;
            this._localization = localization;
            this._userActivity = userActivity;
            this._notifier = notifier;
            this._permissions = permissions;
            this._settings = settings;
            this._dateTimeHelper = dateTimeHelper;
        }

        public IComponentContext Container { get { return _container; } }
        public ICacheManager Cache { get { return _cache.Value; } }
        public IDbContext DbContext { get { return _dbContext.Value; } }
        public ISiteContext SiteContext { get { return _siteContext.Value; } }
        public IWebHelper WebHelper { get { return _webHelper.Value; } }
        public IWorkContext WorkContext { get { return _workContext.Value; } }
        public IEventPublisher EventPublisher { get { return _eventPublisher.Value; } }
        public ILocalizationService Localization { get { return _localization.Value; } }
        public IUserActivityService UserActivity { get { return _userActivity.Value; } }
        public INotifier Notifier { get { return _notifier.Value; } }
        public IPermissionService Permissions { get { return _permissions.Value; } }
        public ISettingService Settings { get { return _settings.Value; } }
        public IDateTimeHelper DateTimeHelper { get { return _dateTimeHelper.Value; } }
    }
}
