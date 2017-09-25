//Contributor:  Nicholas Mayne


namespace InSearch.Services.Authentication.External
{
    public partial interface IClaimsTranslator<T>
    {
        UserClaims Translate(T response);
    }
}