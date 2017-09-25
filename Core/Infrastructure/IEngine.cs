using System;
using InSearch.Core.Infrastructure.DependencyManagement;

namespace InSearch
{
    public interface IEngine
    {
        ContainerManager ContainerManager { get; }

        /// <summary>
        /// Initialize components and plugins in the SmartStore environment.
        /// </summary>
        void Initialize();

        T Resolve<T>(string name = null) where T : class;

        object Resolve(Type type, string name = null);

        T[] ResolveAll<T>();
    }
}
