using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Extensions;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;

namespace Orchard.OutputCache.Filters {
    public class OutputCacheFilter : FilterProvider, IActionFilter, IResultFilter {

        private readonly ICacheManager _cacheManager;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly ITagCache _tagCache;
        private readonly IDisplayedContentItemHandler _displayedContentItemHandler;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IThemeManager _themeManager;
        private readonly IClock _clock;
        private readonly ICacheService _cacheService;
        private readonly ISignals _signals;
        private readonly ShellSettings _shellSettings;
        private readonly ICacheControlStrategy _cacheControlStrategy;
        private Stream _previousFilter;

        public OutputCacheFilter(
            ICacheManager cacheManager,
            IOutputCacheStorageProvider cacheStorageProvider,
            ITagCache tagCache,
            IDisplayedContentItemHandler displayedContentItemHandler,
            IWorkContextAccessor workContextAccessor,
            IThemeManager themeManager,
            IClock clock,
            ICacheService cacheService,
            ISignals signals,
            ShellSettings shellSettings,
            ICacheControlStrategy cacheControlStrategy
            ) {
            _cacheManager = cacheManager;
            _cacheStorageProvider = cacheStorageProvider;
            _tagCache = tagCache;
            _displayedContentItemHandler = displayedContentItemHandler;
            _workContextAccessor = workContextAccessor;
            _themeManager = themeManager;
            _clock = clock;
            _cacheService = cacheService;
            _signals = signals;
            _shellSettings = shellSettings;
            _cacheControlStrategy = cacheControlStrategy;

            Logger = NullLogger.Instance;
        }

        private bool _debugMode;
        private int _cacheDuration;
        private int _maxAge;
        private string _ignoredUrls;
        private bool _applyCulture;
        private string _cacheKey;
        private string _invariantCacheKey;
        private DateTime _now;
        private string[] _varyQueryStringParameters;
        private ISet<string> _varyRequestHeaders;
        private bool _transformRedirect;

        private WorkContext _workContext;
        private CapturingResponseFilter _filter;
        private CacheItem _cacheItem;


        public ILogger Logger { get; set; }

        public void OnActionExecuting(ActionExecutingContext filterContext) {

            // apply OutputCacheAttribute logic if defined
            var actionAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var controllerAttributes = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var outputCacheAttribute = actionAttributes.Concat(controllerAttributes).Cast<OutputCacheAttribute>().FirstOrDefault();

            if (outputCacheAttribute != null) {
                if (outputCacheAttribute.Duration <= 0 || outputCacheAttribute.NoStore) {
                    Logger.Debug("Request ignored based on OutputCache attribute");
                    return;
                }
            }

            // saving the current datetime
            _now = _clock.UtcNow;

            // before executing an action, we check if a valid cached result is already 
            // existing for this context (url, theme, culture, tenant)

            Logger.Debug("Request on: " + filterContext.RequestContext.HttpContext.Request.RawUrl);

            // don't cache POST requests
            if (filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                Logger.Debug("Request ignored on POST");
                return;
            }

            // don't cache the admin
            if (AdminFilter.IsApplied(new RequestContext(filterContext.HttpContext, new RouteData()))) {
                Logger.Debug("Request ignored on Admin section");
                return;
            }

            // ignore child actions, e.g. HomeController is using RenderAction()
            if (filterContext.IsChildAction) {
                Logger.Debug("Request ignored on Child actions");
                return;
            }

            _workContext = _workContextAccessor.GetContext();

            // don't return any cached content, or cache any content, if the user is authenticated
            if (_workContext.CurrentUser != null) {
                Logger.Debug("Request ignored on Authenticated user");
                return;
            }


            // caches the default cache duration to prevent a query to the settings
            _cacheDuration = _cacheManager.Get("CacheSettingsPart.Duration",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    return _workContext.CurrentSite.As<CacheSettingsPart>().DefaultCacheDuration;
                }
            );

            // caches the default max age duration to prevent a query to the settings
            _maxAge = _cacheManager.Get("CacheSettingsPart.MaxAge",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    return _workContext.CurrentSite.As<CacheSettingsPart>().DefaultMaxAge;
                }
            );

            _varyQueryStringParameters = _cacheManager.Get("CacheSettingsPart.VaryQueryStringParameters",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    var varyQueryStringParameters = _workContext.CurrentSite.As<CacheSettingsPart>().VaryQueryStringParameters;

                    return string.IsNullOrWhiteSpace(varyQueryStringParameters) ? null
                        : varyQueryStringParameters.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                }
            );

            var varyRequestHeadersFromSettings = _cacheManager.Get("CacheSettingsPart.VaryRequestHeaders",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    var varyRequestHeaders = _workContext.CurrentSite.As<CacheSettingsPart>().VaryRequestHeaders;

                    return string.IsNullOrWhiteSpace(varyRequestHeaders) ? null
                        : varyRequestHeaders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                }
            );

            _varyRequestHeaders = (varyRequestHeadersFromSettings == null) ? new HashSet<string>() : new HashSet<string>(varyRequestHeadersFromSettings);

            // different tenants with the same urls have different entries
            _varyRequestHeaders.Add("HOST");

            // Set the Vary: Accept-Encoding response header. 
            // This instructs the proxies to cache two versions of the resource: one compressed, and one uncompressed. 
            // The correct version of the resource is delivered based on the client request header. 
            // This is a good choice for applications that are singly homed and depend on public proxies for user locality.
            _varyRequestHeaders.Add("Accept-Encoding");

            // caches the ignored urls to prevent a query to the settings
            _ignoredUrls = _cacheManager.Get("CacheSettingsPart.IgnoredUrls",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    return _workContext.CurrentSite.As<CacheSettingsPart>().IgnoredUrls;
                }
            );

            // caches the culture setting
            _applyCulture = _cacheManager.Get("CacheSettingsPart.ApplyCulture",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    return _workContext.CurrentSite.As<CacheSettingsPart>().ApplyCulture;
                }
            );

            // caches the debug mode
            _debugMode = _cacheManager.Get("CacheSettingsPart.DebugMode",
                context => {
                    context.Monitor(_signals.When(CacheSettingsPart.CacheKey));
                    return _workContext.CurrentSite.As<CacheSettingsPart>().DebugMode;
                }
            );
            
            // don't cache ignored url ?
            if (IsIgnoredUrl(filterContext.RequestContext.HttpContext.Request.AppRelativeCurrentExecutionFilePath, _ignoredUrls)) {
                return;
            }

            var queryString = filterContext.RequestContext.HttpContext.Request.QueryString;
            var requestHeaders = filterContext.RequestContext.HttpContext.Request.Headers;
            var parameters = new Dictionary<string, object>(filterContext.ActionParameters);

            foreach (var key in queryString.AllKeys) {
                if (key == null) continue;

                parameters[key] = queryString[key];
            }

            foreach (var varyByRequestHeader in _varyRequestHeaders) {
                if (requestHeaders.AllKeys.Contains(varyByRequestHeader)) {
                    parameters["HEADER:" + varyByRequestHeader] = requestHeaders[varyByRequestHeader];
                }
            }

            // compute the cache key
            _cacheKey = ComputeCacheKey(filterContext, parameters);

            // create a tag which doesn't care about querystring
            _invariantCacheKey = ComputeCacheKey(filterContext, null);

            // don't retrieve cache content if refused
            // in this case the result of the action will update the current cached version
            if (filterContext.RequestContext.HttpContext.Request.Headers["Cache-Control"] != "no-cache") {

                // fetch cached data
                _cacheItem = _cacheStorageProvider.GetCacheItem(_cacheKey);

                if (_cacheItem == null) {
                    Logger.Debug("Cached version not found");
                }
            }
            else {
                Logger.Debug("Cache-Control = no-cache requested");
            }

            var response = filterContext.HttpContext.Response;

            // render cached content
            if (_cacheItem != null) {
                Logger.Debug("Cache item found, expires on " + _cacheItem.ValidUntilUtc);

                var output = _cacheItem.Output;

                // adds some caching information to the output if requested
                if (_debugMode) {
                    response.AddHeader("X-Cached-On", _cacheItem.CachedOnUtc.ToString("r"));
                    response.AddHeader("X-Cached-Until", _cacheItem.ValidUntilUtc.ToString("r"));
                }

                // shorcut action execution
                filterContext.Result = new ContentResult {
                    Content = output,
                    ContentType = _cacheItem.ContentType
                };

                response.StatusCode = _cacheItem.StatusCode;

                ApplyCacheControl(_cacheItem, response);

                return;
            }

            _cacheItem = new CacheItem();

            // get contents 
            ApplyCacheControl(_cacheItem, response);

            // no cache content available, intercept the execution results for caching
            _previousFilter = response.Filter;
            response.Filter = _filter = new CapturingResponseFilter();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            // handle redirections
            _transformRedirect = TransformRedirect(filterContext);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

            var response = filterContext.HttpContext.Response;

            if (!_cacheControlStrategy.IsCacheable(filterContext.Result, response)) {
                _filter = null;
                if (_previousFilter != null) {
                    response.Filter = _previousFilter;
                } 
                return;
            }
            
            // ignore error results from cache
            if (response.StatusCode != (int)HttpStatusCode.OK) {

                // Never cache non-200 responses.
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();
                filterContext.HttpContext.Response.Cache.SetMaxAge(new TimeSpan(0));

                _filter = null;
                if (_previousFilter != null) {
                    response.Filter = _previousFilter;
                } 
                return;
            }

            // if the result of a POST is a Redirect, remove any Cache Item for this url
            // so that the redirected client gets a fresh result
            // also add a random token to the query string so that public cachers (IIS, proxies, ...) don't return cached content
            // i.e., Comment creation

            // ignore in admin
            if (AdminFilter.IsApplied(new RequestContext(filterContext.HttpContext, new RouteData()))) {
                _filter = null;
                if (_previousFilter != null) {
                    response.Filter = _previousFilter;
                } 
                return;
            }

            _workContext = _workContextAccessor.GetContext();

            // ignore authenticated requests
            if (_workContext.CurrentUser != null) {
                _filter = null;
                if (_previousFilter != null) {
                    response.Filter = _previousFilter;
                } 
                return;
            }

            // save the result only if the content can be intercepted
            if (_filter == null) return;

            // flush here to force the Filter to get the rendered content
            if (response.IsClientConnected)
                response.Flush();

            var output = _filter.GetContents(response.ContentEncoding);

            if (String.IsNullOrWhiteSpace(output)) {
                return;
            }

            response.Filter = null;
            response.Write(output);
            
            // check if there is a specific rule not to cache the whole route
            var configurations = _cacheService.GetRouteConfigurations();
            var route = filterContext.Controller.ControllerContext.RouteData.Route;
            var key = _cacheService.GetRouteDescriptorKey(filterContext.HttpContext, route);
            var configuration = configurations.FirstOrDefault(c => c.RouteKey == key);


            // do not cache ?
            if (configuration != null && configuration.Duration == 0) {
                return;
            }

            // don't cache the result of a POST redirection as it could contain notifications
            if (_transformRedirect) {
                return;
            }

            // don't cache the result if there were some notifications
            var messagesZone = _workContextAccessor.GetContext(filterContext).Layout.Zones["Messages"];
            var hasNotifications = messagesZone != null && ((IEnumerable<dynamic>)messagesZone).Any();
            if (hasNotifications) {
                return;
            }

            // default duration of specific one ?
            var cacheDuration = configuration != null && configuration.Duration.HasValue ? configuration.Duration.Value : _cacheDuration;

            if (cacheDuration <= 0) {
                return;
            }

            // include each of the content item ids as tags for the cache entry
            var contentItemIds = _displayedContentItemHandler.GetDisplayed().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

            if (filterContext.HttpContext.Request.Url == null) {
                return;
            }

            _cacheItem.ContentType = response.ContentType;
            _cacheItem.StatusCode = response.StatusCode;
            _cacheItem.CachedOnUtc = _now;
            _cacheItem.ValidFor = cacheDuration;
            _cacheItem.QueryString = filterContext.HttpContext.Request.Url.Query;
            _cacheItem.Output = output;
            _cacheItem.CacheKey = _cacheKey;
            _cacheItem.InvariantCacheKey = _invariantCacheKey;
            _cacheItem.Tenant = _shellSettings.Name;
            _cacheItem.Url = filterContext.HttpContext.Request.Url.AbsolutePath;
            _cacheItem.Tags = new[] { _invariantCacheKey }.Union(contentItemIds).ToArray();

            Logger.Debug("Cache item added: " + _cacheItem.CacheKey);

            // remove old cache data
            _cacheService.RemoveByTag(_invariantCacheKey);

            // add data to cache
            _cacheStorageProvider.Set(_cacheKey, _cacheItem);

            // add to the tags index
            foreach (var tag in _cacheItem.Tags) {
                _tagCache.Tag(tag, _cacheKey);
            }

        }

        private bool TransformRedirect(ActionExecutedContext filterContext) {

            // removes the target of the redirection from cache after a POST

            if (filterContext.Result == null) {
                throw new ArgumentNullException();
            }

            // status code can't be tested at this point, so test the result type instead
            if (!filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)
                || !(filterContext.Result is RedirectResult)) {
                return false;
            }

            Logger.Debug("Redirect on POST");
            var redirectUrl = ((RedirectResult)(filterContext.Result)).Url ;

            if (!VirtualPathUtility.IsAbsolute(redirectUrl)) {
                var applicationRoot = new UrlHelper(filterContext.HttpContext.Request.RequestContext).MakeAbsolute("/");
                if (redirectUrl.StartsWith(applicationRoot, StringComparison.OrdinalIgnoreCase)) {
                    redirectUrl = "~/" + redirectUrl.Substring(applicationRoot.Length);
                }
            }

            // querystring invariant key
            var invariantCacheKey = ComputeCacheKey(
                _shellSettings.Name,
                redirectUrl,
                () => _workContext.CurrentCulture,
                _themeManager.GetRequestTheme(filterContext.RequestContext).Id,
                null
                );

            _cacheService.RemoveByTag(invariantCacheKey);

            filterContext.Result = new RedirectResult(redirectUrl, ((RedirectResult) filterContext.Result).Permanent);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            return true;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {

        }

        /// <summary>
        /// Define valid cache control values
        /// </summary>
        private void ApplyCacheControl(CacheItem cacheItem, HttpResponseBase response) {
            if (_maxAge > 0) {
                var maxAge = new TimeSpan(0, 0, 0, _maxAge); //cacheItem.ValidUntilUtc - _clock.UtcNow;
                if (maxAge.TotalMilliseconds < 0) {
                    maxAge = TimeSpan.FromSeconds(0);
                }

                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetMaxAge(maxAge);
            }

            // an ETag is a string that uniquely identifies a specific version of a component.
            // we use the cache item to detect if it's a new one
            if (HttpRuntime.UsingIntegratedPipeline) {
                if (response.Headers.Get("ETag") == null) {
                    response.Cache.SetETag(cacheItem.GetHashCode().ToString(CultureInfo.InvariantCulture));
                }
            }

            response.Cache.SetOmitVaryStar(true);

            if (_varyQueryStringParameters != null) {
                foreach (var queryStringParam in _varyQueryStringParameters) {
                    response.Cache.VaryByParams[queryStringParam] = true;
                }
            }

            foreach (var varyRequestHeader in _varyRequestHeaders) {
                response.Cache.VaryByHeaders[varyRequestHeader] = true;
            }

            // create a unique cache per browser, in case a Theme is rendered differently (e.g., mobile)
            // c.f. http://msdn.microsoft.com/en-us/library/aa478965.aspx
            // c.f. http://stackoverflow.com/questions/6007287/outputcache-varybyheader-user-agent-or-varybycustom-browser
            response.Cache.SetVaryByCustom("browser");

            // enabling this would create an entry for each different browser sub-version
            // response.Cache.VaryByHeaders.UserAgent = true;

        }

        private string ComputeCacheKey(ControllerContext controllerContext, IEnumerable<KeyValuePair<string, object>> parameters) {
            var url = controllerContext.HttpContext.Request.RawUrl;
            if (!VirtualPathUtility.IsAbsolute(url)) {
                var applicationRoot = new UrlHelper(controllerContext.HttpContext.Request.RequestContext).MakeAbsolute("/");
                if (url.StartsWith(applicationRoot, StringComparison.OrdinalIgnoreCase)) {
                    url = url.Substring(applicationRoot.Length);
                }
            }
            return ComputeCacheKey(_shellSettings.Name, url, () => _workContext.CurrentCulture, _themeManager.GetRequestTheme(controllerContext.RequestContext).Id, parameters);
        }

        private string ComputeCacheKey(string tenant, string absoluteUrl, Func<string> culture, string theme, IEnumerable<KeyValuePair<string, object>> parameters) {
            var keyBuilder = new StringBuilder();

            keyBuilder.Append("tenant=").Append(tenant).Append(";");

            keyBuilder.Append("url=").Append(absoluteUrl.ToLowerInvariant()).Append(";");

            // include the theme in the cache key
            if (_applyCulture) {
                keyBuilder.Append("culture=").Append(culture().ToLowerInvariant()).Append(";");
            }

            // include the theme in the cache key
            keyBuilder.Append("theme=").Append(theme.ToLowerInvariant()).Append(";");

            if (parameters != null) {
                foreach (var pair in parameters) {
                    keyBuilder.AppendFormat("{0}={1};", pair.Key.ToLowerInvariant(), Convert.ToString(pair.Value).ToLowerInvariant());
                }
            }

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Returns true if the given url should be ignored, as defined in the settings
        /// </summary>
        private static bool IsIgnoredUrl(string url, string ignoredUrls) {
            if (String.IsNullOrEmpty(ignoredUrls)) {
                return false;
            }

            // remove ~ if present
            if (url.StartsWith("~")) {
                url = url.Substring(1);
            }

            using (var urlReader = new StringReader(ignoredUrls)) {
                string relativePath;
                while (null != (relativePath = urlReader.ReadLine())) {
                    // remove ~ if present
                    if (relativePath.StartsWith("~")) {
                        relativePath = relativePath.Substring(1);
                    }

                    if (String.IsNullOrWhiteSpace(relativePath)) {
                        continue;
                    }

                    relativePath = relativePath.Trim();

                    // ignore comments
                    if (relativePath.StartsWith("#")) {
                        continue;
                    }

                    if (String.Equals(relativePath, url, StringComparison.OrdinalIgnoreCase)) {
                        return true;
                    }
                }
            }

            return false;
        }

    }

    /// <summary>
    /// Captures the response stream while writing to it
    /// </summary>
    public class CapturingResponseFilter : Stream {
        private readonly MemoryStream _mem;

        public CapturingResponseFilter() {
            _mem = new MemoryStream();
        }

        // The following members of Stream must be overriden.
        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override long Length {
            get { return 0; }
        }

        public override long Position { get; set; }

        public override long Seek(long offset, SeekOrigin direction) {
            return 0;
        }

        public override void SetLength(long length) {
        }

        public override void Close() {
            _mem.Close();
        }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return count;
        }

        // Override the Write method to filter Response to a file. 
        public override void Write(byte[] buffer, int offset, int count) {

            //Write out the response to the file.
            _mem.Write(buffer, 0, count);
        }

        public string GetContents(Encoding enc) {
            var buffer = new byte[_mem.Length];
            _mem.Position = 0;
            _mem.Read(buffer, 0, buffer.Length);
            return enc.GetString(buffer, 0, buffer.Length);
        }

        public byte[] GetContents() {
            return _mem.ToArray();
        }

        protected override void Dispose(bool disposing) {
            _mem.Dispose();
        }
    }

    public class ViewDataContainer : IViewDataContainer {
        public ViewDataDictionary ViewData { get; set; }
    }

}