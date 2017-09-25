using InSearch.Core.Domain.Users;

namespace InSearch.Services.Authentication
{
    public partial interface IAuthenticationService
    {
        void SignIn(User user, bool createPersistentCookie);
        void SignOut();
        User GetAuthenticatedUser();
    }
}
