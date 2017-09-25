using InSearch.Core.Domain.Security;
using System.Collections.Generic;

namespace InSearch.Services.Security
{
    public interface IPermissionProvider
    {
        IEnumerable<PermissionRecord> GetPermissions();
        IEnumerable<DefaultPermissionRecord> GetDefaultPermissions();
    }
}
