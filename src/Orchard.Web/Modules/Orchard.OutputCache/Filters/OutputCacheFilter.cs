using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Filters;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.Utility.Extensions;

namespace Orchard.OutputCache.Filters {
    public class OutputCacheFilter : FilterProvider, IActionFilter, IResultFilter {

        private static string _refreshKey = "__r";
        private static long _epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;
        private static readonly ConcurrentDictionary<string, object> _cacheKeyLocks = new ConcurrentDictionary<string, object>();

        // Dependencies.
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
        public ILogger Logger { get; set; }

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
            ShellSettings shellSettings) {

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

            Logger = NullLogger.Instance;
        }

        // State.
        private CacheSettings _cacheSettings;
        private DateTime _now;
        private WorkContext _workContext;
        private string _cacheKey;
        private string _invariantCacheKey;
        private bool _transformRedirect;
        private bool _isCachingRequest;

        public void OnActionExecuting(ActionExecutingContext filterContext) {

            Logger.Debug("Incoming request for URL '{0}'.", filterContext.RequestContext.HttpContext.Request.RawUrl);

            // This filter is not reentrant (multiple executions within the same request are
            // not supported) so child actions are ignored completely.
            if (filterContext.IsChildAction) {
                Logger.Debug("Action '{0}' ignored because it's a child action.", filterContext.ActionDescriptor.ActionName);
                return;
            }

            _now = _clock.UtcNow;
            _workContext = _workContextAccessor.GetContext();

            if (!RequestIsCacheable(filterContext))
                return;

            // Computing the cache key after we know that the request is cacheable means that we are only performing this calculation on requests that require it
            _cacheKey = ComputeCacheKey(filterContext, GetCacheKeyParameters(filterContext));
            _invariantCacheKey = ComputeCacheKey(filterContext, null);

            Logger.Debug("Cache key '{0}' was created.", _cacheKey);

            // The cache key lock for a given cache key is used to synchronize requests to
            // ensure only a single request is regenerating the item.
            var cacheKeyLock = _cacheKeyLocks.GetOrAdd(_cacheKey, x => new object());

            try {

                // Is there a cached item, and are we allowed to serve it?
                var allowServeFromCache = filterContext.RequestContext.HttpContext.Request.Headers["Cache-Control"] != "no-cache" || CacheSettings.IgnoreNoCache;
                var cacheItem = GetCacheItem(_cacheKey);
                if (allowServeFromCache && cacheItem != null) {

                    Logger.Debug("Item '{0}' was found in cache.", _cacheKey);

                    // Is the cached item in its grace period?
                    if (cacheItem.IsInGracePeriod(_now)) {

                        // Render the content unless another request is already doing so.
                        if (Monitor.TryEnter(cacheKeyLock)) {
                            Logger.Debug("Item '{0}' is in grace period and not currently being rendered; rendering item...", _cacheKey);
                            BeginRenderItem(filterContext);
                            return;
                        }
                    }

                    // Cached item is not yet in its grace period, or is already being
                    // rendered by another request; serve it from cache.
                    Logger.Debug("Serving item '{0}' from cache.", _cacheKey);
                    ServeCachedItem(filterContext, cacheItem);
                    return;
                }

                // No cached item found, or client doesn't want it; acquire the cache key
                // lock to render the item.
                Logger.Debug("Item '{0}' was not found in cache or client refuses it. Acquiring cache key lock...", _cacheKey);
                if (Monitor.TryEnter(cacheKeyLock, TimeSpan.FromSeconds(20))) {
                    Logger.Debug("Cache key lock for item '{0}' was acquired.", _cacheKey);

                    // Item might now have been rendered and cached by another request; if so serve it from cache.
                    if (allowServeFromCache) {
                        cacheItem = GetCacheItem(_cacheKey);
                        if (cacheItem != null) {
                            Logger.Debug("Item '{0}' was now found; releasing cache key lock and serving from cache.", _cacheKey);
                            Monitor.Exit(cacheKeyLock);
                            ServeCachedItem(filterContext, cacheItem);
                            return;
                        }
                    }
                }

                // Either we acquired the cache key lock and the item was still not in cache, or
                // the lock acquisition timed out. In either case render the item.
                Logger.Debug("Rendering item '{0}'...", _cacheKey);
                BeginRenderItem(filterContext);

            }
            catch {
                // Remember to release the cache key lock in the event of an exception!
                Logger.Debug("Exception occurred for item '{0}'; releasing any acquired lock.", _cacheKey);
                if (Monitor.IsEntered(cacheKeyLock))
                    Monitor.Exit(cacheKeyLock);
                throw;
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            _transformRedirect = TransformRedirect(filterContext);
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

            var captureHandlerIsAttached = false;

            try {

                // This filter is not reentrant (multiple executions within the same request are
                // not supported) so child actions are ignored completely.
                if (filterContext.IsChildAction || !_isCachingRequest)
                    return;

                Logger.Debug("Item '{0}' was rendered.", _cacheKey);

                // Obtain individual route configuration, if any.
                CacheRouteConfig configuration = null;
                var configurations = _cacheService.GetRouteConfigs();
                if (configurations.Any()) {
                    var route = filterContext.Controller.ControllerContext.RouteData.Route;
                    var key = _cacheService.GetRouteDescriptorKey(filterContext.HttpContext, route);
                    configuration = configurations.FirstOrDefault(c => c.RouteKey == key);
                }

                if (!ResponseIsCacheable(filterContext, configuration)) {
                    filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    filterContext.HttpContext.Response.Cache.SetNoStore();
                    filterContext.HttpContext.Response.Cache.SetMaxAge(new TimeSpan(0));
                    return;
                }

                // Determine duration and grace time.
                var cacheDuration = configuration != null && configuration.Duration.HasValue ? configuration.Duration.Value : CacheSettings.DefaultCacheDuration;
                var cacheGraceTime = configuration != null && configuration.GraceTime.HasValue ? configuration.GraceTime.Value : CacheSettings.DefaultCacheGraceTime;

                // Include each content item ID as tags for the cache entry.
                var contentItemIds = _displayedContentItemHandler.GetDisplayed().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

                // Capture the response output using a custom filter stream.
                var response = filterContext.HttpContext.Response;
                var captureStream = new CaptureStream(response.Filter);
                response.Filter = captureStream;
                captureStream.Captured += (output) => {
                    try {
                        // Since this is a callback any call to injected dependencies can result in an Autofac exception: "Instances 
                        // cannot be resolved and nested lifetimes cannot be created from this LifetimeScope as it has already been disposed."
                        // To prevent access to the original lifetime scope a new work context scope should be created here and dependencies
                        // should be resolved from it.

                        using (var scope = _workContextAccessor.CreateWorkContextScope()) {
                            var cacheItem = new CacheItem() {
                                CachedOnUtc = _now,
                                Duration = cacheDuration,
                                GraceTime = cacheGraceTime,
                                Output = output,
                                ContentType = response.ContentType,
                                QueryString = filterContext.HttpContext.Request.Url.Query,
                                CacheKey = _cacheKey,
                                InvariantCacheKey = _invariantCacheKey,
                                Url = filterContext.HttpContext.Request.Url.AbsolutePath,
                                Tenant = scope.Resolve<ShellSettings>().Name,
                                StatusCode = response.StatusCode,
                                Tags = new[] { _invariantCacheKey }.Union(contentItemIds).ToArray()
                            };

                            // Write the rendered item to the cache.
                            var cacheStorageProvider = scope.Resolve<IOutputCacheStorageProvider>();
                            cacheStorageProvider.Remove(_cacheKey);
                            cacheStorageProvider.Set(_cacheKey, cacheItem);

                            Logger.Debug("Item '{0}' was written to cache.", _cacheKey);

                            // Also add the item tags to the tag cache.
                            var tagCache = scope.Resolve<ITagCache>();
                            foreach (var tag in cacheItem.Tags) {
                                tagCache.Tag(tag, _cacheKey);
                            }
                        }
                    }
                    finally {
                        // Always release the cache key lock when the request ends.
                        ReleaseCacheKeyLock();
                    }
                };

                captureHandlerIsAttached = true;
            }
            finally {
                // If the response filter stream capture handler was attached then we'll trust
                // it to release the cache key lock at some point in the future when the stream
                // is flushed; otherwise we'll make sure we'll release it here.
                if (!captureHandlerIsAttached)
                    ReleaseCacheKeyLock();
            }
        }

        protected virtual bool RequestIsCacheable(ActionExecutingContext filterContext) {

            var itemDescriptor = string.Empty;

            if (Logger.IsEnabled(LogLevel.Debug)) {
                var url = filterContext.RequestContext.HttpContext.Request.RawUrl;
                var area = filterContext.RequestContext.RouteData.Values["area"];
                var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var action = filterContext.ActionDescriptor.ActionName;
                var culture = _workContext.CurrentCulture.ToLowerInvariant();
                var auth = filterContext.HttpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant();
                var theme = _themeManager.GetRequestTheme(filterContext.RequestContext).Id.ToLowerInvariant();

                itemDescriptor = string.Format("{0} (Area: {1}, Controller: {2}, Action: {3}, Culture: {4}, Theme: {5}, Auth: {6})", url, area, controller, action, culture, theme, auth);
            }

            // Respect OutputCacheAttribute if applied.
            var actionAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var controllerAttributes = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var outputCacheAttribute = actionAttributes.Concat(controllerAttributes).Cast<OutputCacheAttribute>().FirstOrDefault();
            if (outputCacheAttribute != null) {
                if (outputCacheAttribute.Duration <= 0 || outputCacheAttribute.NoStore) {
                    Logger.Debug("Request for item '{0}' ignored based on OutputCache attribute.", itemDescriptor);
                    return false;
                }
            }

            // Don't cache POST requests.
            if (filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                Logger.Debug("Request for item '{0}' ignored because HTTP method is POST.", itemDescriptor);
                return false;
            }

            // Don't cache admin section requests.
            if (AdminFilter.IsApplied(new RequestContext(filterContext.HttpContext, new RouteData()))) {
                Logger.Debug("Request for item '{0}' ignored because it's in admin section.", itemDescriptor);
                return false;
            }

            // Ignore authenticated requests unless the setting to cache them is true.
            if (_workContext.CurrentUser != null && !CacheSettings.CacheAuthenticatedRequests) {
                Logger.Debug("Request for item '{0}' ignored because user is authenticated.", itemDescriptor);
                return false;
            }

            // Don't cache ignored URLs.
            if (IsIgnoredUrl(filterContext.RequestContext.HttpContext.Request.AppRelativeCurrentExecutionFilePath, CacheSettings.IgnoredUrls)) {
                Logger.Debug("Request for item '{0}' ignored because the URL is configured as ignored.", itemDescriptor);
                return false;
            }

            // Ignore requests with the refresh key on the query string.
            foreach (var key in filterContext.RequestContext.HttpContext.Request.QueryString.AllKeys) {
                if (String.Equals(_refreshKey, key, StringComparison.OrdinalIgnoreCase)) {
                    Logger.Debug("Request for item '{0}' ignored because refresh key was found on query string.", itemDescriptor);
                    return false;
                }
            }

            return true;
        }

        protected virtual bool ResponseIsCacheable(ResultExecutedContext filterContext, CacheRouteConfig configuration) {

            if (filterContext.HttpContext.Request.Url == null) {
                return false;
            }

            // Don't cache non-200 responses or results of a redirect.
            var response = filterContext.HttpContext.Response;
            if (response.StatusCode != (int)HttpStatusCode.OK || _transformRedirect) {
                return false;
            }

            // Don't cache in individual route configuration says no.
            if (configuration != null && configuration.Duration == 0) {
                Logger.Debug("Response for item '{0}' will not be cached because route is configured to not be cached.", _cacheKey);
                return false;
            }

            // Don't cache if request created notifications.
            var hasNotifications = !String.IsNullOrEmpty(Convert.ToString(filterContext.Controller.TempData["messages"]));
            if (hasNotifications) {
                Logger.Debug("Response for item '{0}' will not be cached because one or more notifications were created.", _cacheKey);
                return false;
            }

            return true;
        }

        protected virtual IDictionary<string, object> GetCacheKeyParameters(ActionExecutingContext filterContext) {
            var result = new Dictionary<string, object>();
            
            // Vary by action parameters.
            foreach (var p in filterContext.ActionParameters)
                result.Add("PARAM:" + p.Key, p.Value);

            // Vary by theme.
            result.Add("theme", _themeManager.GetRequestTheme(filterContext.RequestContext).Id.ToLowerInvariant());

            // Vary by configured query string parameters.
            var queryString = filterContext.RequestContext.HttpContext.Request.QueryString;
            foreach (var key in queryString.AllKeys) {
                if (key == null || (CacheSettings.VaryByQueryStringParameters != null && !CacheSettings.VaryByQueryStringParameters.Contains(key)))
                    continue;
                result[key] = queryString[key];
            }

            // Vary by configured request headers.
            var requestHeaders = filterContext.RequestContext.HttpContext.Request.Headers;
            foreach (var varyByRequestHeader in CacheSettings.VaryByRequestHeaders) {
                if (requestHeaders.AllKeys.Contains(varyByRequestHeader))
                    result["HEADER:" + varyByRequestHeader] = requestHeaders[varyByRequestHeader];
            }

            
            // Vary by request culture if configured.
            if (CacheSettings.VaryByCulture) {
                result["culture"] = _workContext.CurrentCulture.ToLowerInvariant();
            }

            // Vary by authentication state if configured.
            if (CacheSettings.VaryByAuthenticationState) {
                result["auth"] = filterContext.HttpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant();
            }

            return result;
        }

        protected virtual bool TransformRedirect(ActionExecutedContext filterContext) {

            // Removes the target of the redirection from cache after a POST.

            if (filterContext.Result == null) {
                throw new ArgumentNullException();
            }

            if (AdminFilter.IsApplied(new RequestContext(filterContext.HttpContext, new RouteData()))) {
                return false;
            }

            var redirectResult = filterContext.Result as RedirectResult;

            // status code can't be tested at this point, so test the result type instead
            if (redirectResult == null || !filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            Logger.Debug("Redirect on POST detected; removing from cache and adding refresh key.");

            var redirectUrl = redirectResult.Url;

            if (filterContext.HttpContext.Request.IsLocalUrl(redirectUrl)) {
                // Remove all cached versions of the same item.
                var helper = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
                var absolutePath = new Uri(helper.MakeAbsolute(redirectUrl)).AbsolutePath;
                var invariantCacheKey = ComputeCacheKey(_shellSettings.Name, absolutePath, null);
                _cacheService.RemoveByTag(invariantCacheKey);
            }

            // Adding a refresh key so that the redirection doesn't get restored
            // from a cached version on a proxy. This can happen when using public
            // caching, we want to force the client to get a fresh copy of the
            // redirectUrl content.

            if (CacheSettings.DefaultMaxAge > 0) {
                var epIndex = redirectUrl.IndexOf('?');
                var qs = new NameValueCollection();
                if (epIndex > 0) {
                    qs = HttpUtility.ParseQueryString(redirectUrl.Substring(epIndex));
                }

                // Substract Epoch to get a smaller number.
                var refresh = _now.Ticks - _epoch;
                qs.Remove(_refreshKey);

                qs.Add(_refreshKey, refresh.ToString("x"));
                var querystring = "?" + string.Join("&", Array.ConvertAll(qs.AllKeys, k => string.Format("{0}={1}", HttpUtility.UrlEncode(k), HttpUtility.UrlEncode(qs[k]))));

                if (epIndex > 0) {
                    redirectUrl = redirectUrl.Substring(0, epIndex) + querystring;
                }
                else {
                    redirectUrl = redirectUrl + querystring;
                }
            }

            filterContext.Result = new RedirectResult(redirectUrl, redirectResult.Permanent);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            return true;
        }

        private CacheSettings CacheSettings {
            get {
                return _cacheSettings ?? (_cacheSettings = _cacheManager.Get(CacheSettings.CacheKey, true, context => {
                    context.Monitor(_signals.When(CacheSettings.CacheKey));
                    return new CacheSettings(_workContext.CurrentSite.As<CacheSettingsPart>());
                }));
            }
        }

        private void ServeCachedItem(ActionExecutingContext filterContext, CacheItem cacheItem) {
            var response = filterContext.HttpContext.Response;

            // Fix for missing charset in response headers
            response.Charset = response.Charset; 

            // Adds some caching information to the output if requested.
            if (CacheSettings.DebugMode) {
                response.AddHeader("X-Cached-On", cacheItem.CachedOnUtc.ToString("r"));
                response.AddHeader("X-Cached-Until", cacheItem.ValidUntilUtc.ToString("r"));
            }

            // Shorcut action execution.
            filterContext.Result = new FileContentResult(cacheItem.Output, cacheItem.ContentType);

            response.StatusCode = cacheItem.StatusCode;

            ApplyCacheControl(response);
        }

        private void BeginRenderItem(ActionExecutingContext filterContext) {

            var response = filterContext.HttpContext.Response;

            ApplyCacheControl(response);

            // Remember that we should intercept the rendered response output.
            _isCachingRequest = true;
        }

        private void ApplyCacheControl(HttpResponseBase response) {

            if (CacheSettings.DefaultMaxAge > 0) {
                var maxAge = TimeSpan.FromSeconds(CacheSettings.DefaultMaxAge); //cacheItem.ValidUntilUtc - _clock.UtcNow;
                if (maxAge.TotalMilliseconds < 0) {
                    maxAge = TimeSpan.Zero;
                }
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetMaxAge(maxAge);
            }

            // Keeping this example for later usage.
            // response.DisableUserCache();
            // response.DisableKernelCache();
            // response.Cache.SetOmitVaryStar(true);

            // An ETag is a string that uniquely identifies a specific version of a component.
            // We use the cache item to detect if it's a new one.
            if (HttpRuntime.UsingIntegratedPipeline) {
                if (response.Headers.Get("ETag") == null) {
                    // What is the point of GetHashCode() of a newly generated item? /DanielStolt
                    response.Cache.SetETag(new CacheItem().GetHashCode().ToString(CultureInfo.InvariantCulture));
                }
            }

            if (CacheSettings.VaryByQueryStringParameters == null) {
                response.Cache.VaryByParams["*"] = true;
            }
            else {
                foreach (var queryStringParam in CacheSettings.VaryByQueryStringParameters) {
                    response.Cache.VaryByParams[queryStringParam] = true;
                }
            }

            foreach (var varyRequestHeader in CacheSettings.VaryByRequestHeaders) {
                response.Cache.VaryByHeaders[varyRequestHeader] = true;
            }
        }

        private void ReleaseCacheKeyLock() {
            if (_cacheKey != null) {
                object cacheKeyLock;
                if (_cacheKeyLocks.TryGetValue(_cacheKey, out cacheKeyLock) && Monitor.IsEntered(cacheKeyLock)) {
                    Logger.Debug("Releasing cache key lock for item '{0}'.", _cacheKey);
                    Monitor.Exit(cacheKeyLock);
                }
            }
        }

        protected virtual bool IsIgnoredUrl(string url, IEnumerable<string> ignoredUrls) {
            if (ignoredUrls == null || !ignoredUrls.Any()) {
                return false;
            }

            url = url.TrimStart(new[] { '~' });

            foreach (var ignoredUrl in ignoredUrls) {
                var relativePath = ignoredUrl.TrimStart(new[] { '~' }).Trim();
                if (String.IsNullOrWhiteSpace(relativePath)) {
                    continue;
                }

                // Ignore comments
                if (relativePath.StartsWith("#")) {
                    continue;
                }

                if (String.Equals(relativePath, url, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        protected virtual string ComputeCacheKey(ControllerContext controllerContext, IEnumerable<KeyValuePair<string, object>> parameters) {
            var url = controllerContext.HttpContext.Request.Url.AbsolutePath;
            return ComputeCacheKey(_shellSettings.Name, url, parameters);
        }

        protected virtual string ComputeCacheKey(string tenant, string absoluteUrl, IEnumerable<KeyValuePair<string, object>> parameters) {
            var keyBuilder = new StringBuilder();

            keyBuilder.AppendFormat("tenant={0};url={1};", tenant, absoluteUrl.ToLowerInvariant());

            if (parameters != null) {
                foreach (var pair in parameters) {
                    keyBuilder.AppendFormat("{0}={1};", pair.Key.ToLowerInvariant(), Convert.ToString(pair.Value).ToLowerInvariant());
                }
            }

            return keyBuilder.ToString();
        }

        protected virtual CacheItem GetCacheItem(string key) {
            try {
                var cacheItem = _cacheStorageProvider.GetCacheItem(key);
                return cacheItem;
            }
            catch(Exception e) {
                Logger.Error(e, "An unexpected error occured while reading a cache entry");
            }

            return null;
        }
    }

    public class ViewDataContainer : IViewDataContainer {
        public ViewDataDictionary ViewData { get; set; }
    }
}