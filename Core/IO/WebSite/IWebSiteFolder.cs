using System.Collections.Generic;
using System.IO;

namespace InSearch.Core.IO.WebSite
{
    public interface IWebSiteFolder
    {
        IEnumerable<string> ListDirectories(string virtualPath);
        IEnumerable<string> ListFiles(string virtualPath, bool recursive);

        bool FileExists(string virtualPath);
        string ReadFile(string virtualPath);
        void CopyFileTo(string virtualPath, Stream destination);
    }
}
