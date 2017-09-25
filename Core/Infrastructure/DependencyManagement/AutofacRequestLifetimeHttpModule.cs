using System;
using System.Web;
using Autofac.Integration.Mvc;

namespace InSearch.Core.Infrastructure.DependencyManagement
{
    public class AutofacRequestLifetimeHttpModule : IHttpModule
    {
        #region New

        public void Init(HttpApplication context)
        {
            Guard.ArgumentNotNull(() => context);

            context.EndRequest += OnEndRequest;
        }

        public static void OnEndRequest(object sender, EventArgs e)
        {
            if (LifetimeScopeProvider != null)
            {
                LifetimeScopeProvider.EndLifetimeScope();
            }
        }

        public static void SetLifetimeScopeProvider(ILifetimeScopeProvider lifetimeScopeProvider)
        {
            if (lifetimeScopeProvider == null)
            {
                throw new ArgumentNullException("lifetimeScopeProvider");
            }
            LifetimeScopeProvider = lifetimeScopeProvider;
        }

        internal static ILifetimeScopeProvider LifetimeScopeProvider { get; set; }

        public void Dispose() { }
        #endregion New
    }
}
