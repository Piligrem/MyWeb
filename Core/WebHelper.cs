using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using InSearch.Core.Data;
using InSearch.Core.Domain;
using InSearch.Core.Infrastructure;
using InSearch.Utilities;

namespace InSearch.Core
{
    public partial class WebHelper : IWebHelper
    {
        #region Fields
        private static bool? s_optimizedCompilationsEnabled = null;
        private static AspNetHostingPermissionLevel? s_trustLevel = null;
        private static readonly Regex s_staticExts = new Regex(@"(.*?)\.(css|js|png|jpg|jpeg|gif|bmp|html|htm|xml|pdf|doc|xls|rar|zip|ico|eot|svg|ttf|woff|otf|axd|ashx|less)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly HttpContextBase _httpContext;
        private bool? _isCurrentConnectionSecured;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        public WebHelper(HttpContextBase httpContext)
        {
            this._httpContext = httpContext;
        }
        #endregion Constructors

        /// <summary>
        /// Get URL referrer
        /// </summary>
        /// <returns>URL referrer</returns>
        public virtual string GetUrlReferrer()
        {
            string referrerUrl = string.Empty;

            if (_httpContext != null &&
                _httpContext.Request != null &&
                _httpContext.Request.UrlReferrer != null)
                referrerUrl = _httpContext.Request.UrlReferrer.ToString();

            return referrerUrl;
        }

        /// <summary>
        /// Get context IP address
        /// </summary>
        /// <returns>URL referrer</returns>
        public virtual string GetCurrentIpAddress()
        {
            if (_httpContext != null && _httpContext.Request != null)
            {
                return _httpContext.Request.UserHostAddress.EmptyNull();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets this page name
        /// </summary>
        /// <param name="includeQueryString">Value indicating whether to include query strings</param>
        /// <returns>Page name</returns>
        public virtual string GetThisPageUrl(bool includeQueryString)
        {
            bool useSsl = IsCurrentConnectionSecured();
            return GetThisPageUrl(includeQueryString, useSsl);
        }

        /// <summary>
        /// Gets this page name
        /// </summary>
        /// <param name="includeQueryString">Value indicating whether to include query strings</param>
        /// <param name="useSsl">Value indicating whether to get SSL protected page</param>
        /// <returns>Page name</returns>
        public virtual string GetThisPageUrl(bool includeQueryString, bool useSsl)
        {
            string url = string.Empty;
            if (_httpContext == null || _httpContext.Request == null)
                return url;

            if (includeQueryString)
            {
                bool appPathPossiblyAppended;
                string siteHost = GetSiteHost(useSsl, out appPathPossiblyAppended).TrimEnd('/');

                string rawUrl = string.Empty;
                if (appPathPossiblyAppended)
                {
                    string temp = _httpContext.Request.AppRelativeCurrentExecutionFilePath.TrimStart('~');
                    rawUrl = temp;
                }
                else
                {
                    rawUrl = _httpContext.Request.RawUrl;
                }

                url = siteHost + rawUrl;
            }
            else
            {
                if (_httpContext.Request.Url != null)
                {
                    url = _httpContext.Request.Url.GetLeftPart(UriPartial.Path);
                }
            }

            return url.ToLowerInvariant();
        }

        /// <summary>
        /// Gets a value indicating whether current connection is secured
        /// </summary>
        /// <returns>true - secured, false - not secured</returns>
        public virtual bool IsCurrentConnectionSecured()
        {
            if (!_isCurrentConnectionSecured.HasValue)
            {
                _isCurrentConnectionSecured = false;
                if (_httpContext != null && _httpContext.Request != null)
                {
                    _isCurrentConnectionSecured = _httpContext.Request.IsSecureConnection();
                }
            }

            return _isCurrentConnectionSecured.Value;
        }

        /// <summary>
        /// Gets server variable by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Server variable</returns>
        public virtual string ServerVariables(string name)
        {
            string result = string.Empty;

            try
            {
                if (_httpContext != null && _httpContext.Request != null)
                {
                    if (_httpContext.Request.ServerVariables[name] != null)
                    {
                        result = _httpContext.Request.ServerVariables[name];
                    }
                }
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }

        private string GetHostPart(string url)
        {
            var uri = new Uri(url);
            var host = uri.GetComponents(UriComponents.Scheme | UriComponents.Host, UriFormat.Unescaped);
            return host;
        }

        /// <summary>
        /// Gets site host location
        /// </summary>
        /// <param name="useSsl">Use SSL</param>
        /// <param name="appPathPossiblyAppended">
        ///     <c>true</c> when the host url had to be resolved from configuration, 
        ///     where a possible folder name may have been specified (e.g. www.mycompany.com/SHOP)
        /// </param>
        /// <returns>Site host location</returns>
        private string GetSiteHost(bool useSsl, out bool appPathPossiblyAppended)
        {
            appPathPossiblyAppended = false;
            var result = "";
            var httpHost = ServerVariables("HTTP_HOST");

            if (httpHost.HasValue())
            {
                result = "http://" + httpHost.EnsureEndsWith("/");
            }

            if (!DataSettings.DatabaseIsInstalled())
            {
                if (useSsl)
                {
                    // Secure URL is not specified.
                    // So a site owner wants it to be detected automatically.
                    result = result.Replace("http:/", "https:/");
                }
            }
            //***???***
            //else
            //{
            //    //let's resolve IWorkContext  here.
            //    //Do not inject it via constructor because it'll cause circular references
            //    //ISiteContext siteContext;
            //    //if (EngineContext.Current.ContainerManager.TryResolve<ISiteContext>(null, out siteContext)) // Unit test safe!
            //    {
            //        var currentSite = siteContext.CurrentSite;
            //        if (currentSite == null)
            //            throw new Exception("Current site cannot be loaded");

            //        var securityMode = currentSite.GetSecurityMode(useSsl);

            //        if (httpHost.IsEmpty())
            //        {
            //            //HTTP_HOST variable is not available.
            //            //It's possible only when HttpContext is not available (for example, running in a schedule task)
            //            result = currentSite.Url.EnsureEndsWith("/");

            //            appPathPossiblyAppended = true;
            //        }

            //        if (useSsl)
            //        {
            //            if (securityMode == HttpSecurityMode.SharedSsl)
            //            {
            //                // Secure URL for shared SSL specified. 
            //                // So a site owner doesn't want it to be resolved automatically.
            //                // In this case let's use the specified secure URL
            //                result = currentSite.SecureUrl.EmptyNull();

            //                if (!result.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            //                {
            //                    result = "https://" + result;
            //                }

            //                appPathPossiblyAppended = true;
            //            }
            //            else
            //            {
            //                // Secure URL is not specified.
            //                // So a site owner wants it to be resolved automatically.
            //                result = result.Replace("http:/", "https:/");
            //            }
            //        }
            //        else // no SSL
            //        {
            //            if (securityMode == HttpSecurityMode.SharedSsl)
            //            {
            //                // SSL is enabled in this site and shared SSL URL is specified.
            //                // So a site owner doesn't want it to be resolved automatically.
            //                // In this case let's use the specified non-secure URL

            //                result = currentSite.Url;
            //                appPathPossiblyAppended = true;
            //            }
            //        }
            //    }
            //}

            return result.EnsureEndsWith("/").ToLowerInvariant();
        }

        /// <summary>
        /// Gets site location
        /// </summary>
        /// <returns>Site location</returns>
        public virtual string GetSiteLocation()
        {
            bool useSsl = IsCurrentConnectionSecured();
            return GetSiteLocation(useSsl);
        }

        /// <summary>
        /// Gets site location
        /// </summary>
        /// <param name="useSsl">Use SSL</param>
        /// <returns>Site location</returns>
        public virtual string GetSiteLocation(bool useSsl)
        {
            //return HostingEnvironment.ApplicationVirtualPath;

            bool appPathPossiblyAppended;
            string result = GetSiteHost(useSsl, out appPathPossiblyAppended);

            if (result.EndsWith("/"))
            {
                result = result.Substring(0, result.Length - 1);
            }

            if (_httpContext != null && _httpContext.Request != null)
            {
                var appPath = _httpContext.Request.ApplicationPath;
                if (!appPathPossiblyAppended && !result.EndsWith(appPath, StringComparison.OrdinalIgnoreCase))
                {
                    // in a shared SSL scenario the user defined https url could contain
                    // the app path already. In this case we must not append.
                    result = result + appPath;
                }
            }

            if (!result.EndsWith("/"))
            {
                result += "/";
            }

            return result.ToLowerInvariant();
        }

        /// <summary>
        /// Returns true if the requested resource is one of the typical resources that needn't be processed by the cms engine.
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <returns>True if the request targets a static resource file.</returns>
        /// <remarks>
        /// These are - among others - the file extensions considered to be static resources:
        /// .css
        ///	.gif
        /// .png 
        /// .jpg
        /// .jpeg
        /// .js
        /// .axd
        /// .ashx
        /// </remarks>
        public virtual bool IsStaticResource(HttpRequest request)
        {
            return IsStaticResourceRequested(new HttpRequestWrapper(request));
        }

        public static bool IsStaticResourceRequested(HttpRequest request)
        {
            Guard.ArgumentNotNull(() => request);
            return s_staticExts.IsMatch(request.Path);
        }

        public static bool IsStaticResourceRequested(HttpRequestBase request)
        {
            // unit testable
            Guard.ArgumentNotNull(() => request);
            return s_staticExts.IsMatch(request.Path);
        }

        /// <summary>
        /// Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string MapPath(string path)
        {
            return CommonHelper.MapPath(path, false);
        }

        /// <summary>
        /// Modifies query string
        /// </summary>
        /// <param name="url">Url to modify</param>
        /// <param name="queryStringModification">Query string modification</param>
        /// <param name="anchor">Anchor</param>
        /// <returns>New url</returns>
        public virtual string ModifyQueryString(string url, string queryStringModification, string anchor)
        {
            if (url == null)
                url = string.Empty;
            url = url.ToLowerInvariant();

            if (queryStringModification == null)
                queryStringModification = string.Empty;
            queryStringModification = queryStringModification.ToLowerInvariant();

            if (anchor == null)
                anchor = string.Empty;
            anchor = anchor.ToLowerInvariant();


            string str = string.Empty;
            string str2 = string.Empty;
            if (url.Contains("#"))
            {
                str2 = url.Substring(url.IndexOf("#") + 1);
                url = url.Substring(0, url.IndexOf("#"));
            }
            if (url.Contains("?"))
            {
                str = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }
            if (!string.IsNullOrEmpty(queryStringModification))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    var dictionary = new Dictionary<string, string>();
                    foreach (string str3 in str.Split(new char[] { '&' }))
                    {
                        if (!string.IsNullOrEmpty(str3))
                        {
                            string[] strArray = str3.Split(new char[] { '=' });
                            if (strArray.Length == 2)
                            {
                                dictionary[strArray[0]] = strArray[1];
                            }
                            else
                            {
                                dictionary[str3] = null;
                            }
                        }
                    }
                    foreach (string str4 in queryStringModification.Split(new char[] { '&' }))
                    {
                        if (!string.IsNullOrEmpty(str4))
                        {
                            string[] strArray2 = str4.Split(new char[] { '=' });
                            if (strArray2.Length == 2)
                            {
                                dictionary[strArray2[0]] = strArray2[1];
                            }
                            else
                            {
                                dictionary[str4] = null;
                            }
                        }
                    }
                    var builder = new StringBuilder();
                    foreach (string str5 in dictionary.Keys)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append("&");
                        }
                        builder.Append(str5);
                        if (dictionary[str5] != null)
                        {
                            builder.Append("=");
                            builder.Append(dictionary[str5]);
                        }
                    }
                    str = builder.ToString();
                }
                else
                {
                    str = queryStringModification;
                }
            }
            if (!string.IsNullOrEmpty(anchor))
            {
                str2 = anchor;
            }
            return (url + (string.IsNullOrEmpty(str) ? "" : ("?" + str)) + (string.IsNullOrEmpty(str2) ? "" : ("#" + str2))).ToLowerInvariant();
        }

        /// <summary>
        /// Remove query string from url
        /// </summary>
        /// <param name="url">Url to modify</param>
        /// <param name="queryString">Query string to remove</param>
        /// <returns>New url</returns>
        public virtual string RemoveQueryString(string url, string queryString)
        {
            if (url == null)
                url = string.Empty;
            url = url.ToLowerInvariant();

            if (queryString == null)
                queryString = string.Empty;
            queryString = queryString.ToLowerInvariant();


            string str = string.Empty;
            if (url.Contains("?"))
            {
                str = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }
            if (!string.IsNullOrEmpty(queryString))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    var dictionary = new Dictionary<string, string>();
                    foreach (string str3 in str.Split(new char[] { '&' }))
                    {
                        if (!string.IsNullOrEmpty(str3))
                        {
                            string[] strArray = str3.Split(new char[] { '=' });
                            if (strArray.Length == 2)
                            {
                                dictionary[strArray[0]] = strArray[1];
                            }
                            else
                            {
                                dictionary[str3] = null;
                            }
                        }
                    }
                    dictionary.Remove(queryString);

                    var builder = new StringBuilder();
                    foreach (string str5 in dictionary.Keys)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append("&");
                        }
                        builder.Append(str5);
                        if (dictionary[str5] != null)
                        {
                            builder.Append("=");
                            builder.Append(dictionary[str5]);
                        }
                    }
                    str = builder.ToString();
                }
            }
            return (url + (string.IsNullOrEmpty(str) ? "" : ("?" + str)));
        }

        /// <summary>
        /// Gets query string value by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Parameter name</param>
        /// <returns>Query string value</returns>
        public virtual T QueryString<T>(string name)
        {
            string queryParam = null;
            if (_httpContext != null && _httpContext.Request.QueryString[name] != null)
                queryParam = _httpContext.Request.QueryString[name];

            if (!String.IsNullOrEmpty(queryParam))
                return queryParam.Convert<T>();

            return default(T);
        }

        /// <summary>
        /// Restart application domain
        /// </summary>
        /// <param name="makeRedirect">A value indicating whether </param>
        /// <param name="redirectUrl">Redirect URL; empty string if you want to redirect to the current page URL</param>
        public virtual void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "")
        {
            if (WebHelper.GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                //full trust
                HttpRuntime.UnloadAppDomain();

                if (!OptimizedCompilationsEnabled)
                {
                    // not a good idea with optimized compilation!
                    TryWriteGlobalAsax();
                }
            }
            else
            {
                //medium trust
                bool success = TryWriteWebConfig();
                if (!success)
                {
                    throw new InSearchException("InSearch needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'web.config' file.");
                }

                success = TryWriteGlobalAsax();
                if (!success)
                {
                    throw new InSearchException("InSearch needs to be restarted due to a configuration change, but was unable to do so." + Environment.NewLine +
                        "To prevent this issue in the future, a change to the web server configuration is required:" + Environment.NewLine +
                        "- run the application in a full trust environment, or" + Environment.NewLine +
                        "- give the application write access to the 'Global.asax' file.");
                }
            }

            // If setting up extensions/modules requires an AppDomain restart, it's very unlikely the
            // current request can be processed correctly.  So, we redirect to the same URL, so that the
            // new request will come to the newly started AppDomain.
            if (_httpContext != null && makeRedirect)
            {
                if (_httpContext.Request.RequestType == "GET")
                {
                    if (String.IsNullOrEmpty(redirectUrl))
                    {
                        redirectUrl = GetThisPageUrl(true);
                    }
                    _httpContext.Response.Redirect(redirectUrl, true /*endResponse*/);
                }
                else
                {
                    // Don't redirect posts...
                    _httpContext.Response.ContentType = "text/html";
                    _httpContext.Response.WriteFile("~/refresh.html");
                    _httpContext.Response.End();
                }
            }
        }

        private bool TryWriteWebConfig()
        {
            try
            {
                // In medium trust, "UnloadAppDomain" is not supported. Touch web.config
                // to force an AppDomain restart.
                File.SetLastWriteTimeUtc(MapPath("~/web.config"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryWriteGlobalAsax()
        {
            try
            {
                File.SetLastWriteTimeUtc(MapPath("~/global.asax"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryWriteBinFolder()
        {
            try
            {
                var binMarker = MapPath("~/bin/HostRestart");
                Directory.CreateDirectory(binMarker);

                using (var stream = File.CreateText(Path.Combine(binMarker, "marker.txt")))
                {
                    stream.WriteLine("Restart on '{0}'", DateTime.UtcNow);
                    stream.Flush();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool OptimizedCompilationsEnabled
        {
            get
            {
                if (!s_optimizedCompilationsEnabled.HasValue)
                {
                    var section = (CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
                    s_optimizedCompilationsEnabled = section.OptimizeCompilations;
                }

                return s_optimizedCompilationsEnabled.Value;
            }
        }

        /// <summary>
        /// Get a value indicating whether the request is made by search engine (web crawler)
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <returns>Result</returns>
        public virtual bool IsSearchEngine(HttpContextBase context)
        {
            //we accept HttpContext instead of HttpRequest and put required logic in try-catch block
            //more info: http://www.nopcommerce.com/boards/t/17711/unhandled-exception-request-is-not-available-in-this-context.aspx
            if (context == null)
                return false;

            bool result = false;
            try
            {
                if (context.Request.GetType().ToString().Contains("Fake"))
                    return false;

                result = context.Request.Browser.Crawler;
                if (!result)
                {
                    //put any additional known crawlers in the Regex below for some custom validation
                    //var regEx = new Regex("Twiceler|twiceler|BaiDuSpider|baduspider|Slurp|slurp|ask|Ask|Teoma|teoma|Yahoo|yahoo");
                    //result = regEx.Match(request.UserAgent).Success;
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }
            return result;
        }

        /// <summary>
        /// Gets a value that indicates whether the client is being redirected to a new location
        /// </summary>
        public virtual bool IsRequestBeingRedirected
        {
            get
            {
                var response = _httpContext.Response;
                return response.IsRequestBeingRedirected;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the client is being redirected to a new location using POST
        /// </summary>
        public virtual bool IsPostBeingDone
        {
            get
            {
                if (_httpContext.Items["insearch.IsPOSTBeingDone"] == null)
                    return false;
                return Convert.ToBoolean(_httpContext.Items["insearch.IsPOSTBeingDone"]);
            }
            set
            {
                _httpContext.Items["insearch.IsPOSTBeingDone"] = value;
            }
        }

        /// <summary>
        /// Finds the trust level of the running application (http://blogs.msdn.com/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx)
        /// </summary>
        /// <returns>The current trust level.</returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!s_trustLevel.HasValue)
            {
                //set minimum
                s_trustLevel = AspNetHostingPermissionLevel.None;

                //determine maximum
                foreach (AspNetHostingPermissionLevel trustLevel in
                        new AspNetHostingPermissionLevel[] {
                                AspNetHostingPermissionLevel.Unrestricted,
                                AspNetHostingPermissionLevel.High,
                                AspNetHostingPermissionLevel.Medium,
                                AspNetHostingPermissionLevel.Low,
                                AspNetHostingPermissionLevel.Minimal
                            })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        s_trustLevel = trustLevel;
                        break; //we've set the highest permission we can
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return s_trustLevel.Value;
        }

        private class SiteHost
        {
            public string Host { get; set; }
            public bool ExpectingDirtySecurityChannelMove { get; set; }
        }
    }
}
