using System.Net;
using InSearch.Core;
using InSearch.Services.Tasks;
using InSearch.Core.Domain.Common;

namespace InSearch.Services.Common
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveTask : ITask
    {
		private readonly SiteSettings _siteSittings;

        public KeepAliveTask(SiteSettings siteSittings)
        {
            this._siteSittings = siteSittings;
        }

        public void Execute()
        {
            var siteUrl = _siteSittings.Url.TrimEnd('\\').EnsureEndsWith("/");
            string url = siteUrl + "keepalive/index";

            try
            {
                using (var wc = new WebClient())
                {
                    //wc.Headers.Add("InSearch"); // makes problems
                    wc.DownloadString(url);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound) // HTTP 404
                    {
                        // the page was not found (as it can be expected with some web servers)
                        return;
                    }
                }
                // throw any other exception - this should not occur
                throw;
            }
        }
    }
}
