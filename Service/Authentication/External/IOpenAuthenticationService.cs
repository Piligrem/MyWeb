//Contributor:  Nicholas Mayne

using System.Collections.Generic;
using InSearch.Core.Domain.Users;

namespace InSearch.Services.Authentication.External
{
    public partial interface IOpenAuthenticationService
    {
        /// <summary>
        /// Load active external authentication methods
        /// </summary>
        /// <returns>Payment methods</returns>
		IList<IExternalAuthenticationMethod> LoadActiveExternalAuthenticationMethods();

        /// <summary>
        /// Load external authentication method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found external authentication method</returns>
        IExternalAuthenticationMethod LoadExternalAuthenticationMethodBySystemName(string systemName);

        /// <summary>
        /// Load all external authentication methods
        /// </summary>
        /// <returns>External authentication methods</returns>
		IList<IExternalAuthenticationMethod> LoadAllExternalAuthenticationMethods();


        bool AccountExists(OpenAuthenticationParameters parameters);

        void AssociateExternalAccountWithUser(User user, OpenAuthenticationParameters parameters);

        User GetUser(OpenAuthenticationParameters parameters);

        IList<ExternalAuthenticationRecord> GetExternalIdentifiersFor(User user);

        void RemoveAssociation(OpenAuthenticationParameters parameters);
    }
}