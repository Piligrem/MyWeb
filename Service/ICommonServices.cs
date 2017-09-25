using Autofac;
using InSearch.Services.Configuration;
using InSearch.Services.Helpers;
using InSearch.Core;
using InSearch.Core.Caching;
using InSearch.Core.Data;
using InSearch.Core.Events;
using InSearch.Services.Localization;
using InSearch.Core.Logging;
using InSearch.Services.Security;
using InSearch.Services.Logging;

namespace InSearch.Services
{
    public interface ICommonServices
    {
        IComponentContext Container { get; }
        ICacheManager Cache { get; }
        IDbContext DbContext { get; }
        ISiteContext SiteContext { get; }
        IWebHelper WebHelper { get; }
        IWorkContext WorkContext { get; }
        IEventPublisher EventPublisher { get; }
        ILocalizationService Localization { get; }
        IUserActivityService UserActivity { get; }
        INotifier Notifier { get; }
        IPermissionService Permissions { get; }
        ISettingService Settings { get; }
        IDateTimeHelper DateTimeHelper { get; }
    }
}
