using System;
using System.Collections.Generic;
using Orchard.OutputCache.Models;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.FileSystems.AppData;
using Orchard.Environment.Configuration;
using System.Web;
using Orchard.Owin;
using Orchard.Environment;
using Owin;
using Orchard.OutputCache.Filters;
using Orchard.Caching;
using Orchard.Themes;
using Microsoft.Owin;
using Orchard.UI.Admin;
using System.Web.Routing;
using Orchard.ContentManagement;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Orchard.Utility.Extensions;
using System.Web.Mvc;
using Orchard.Mvc.Extensions;
using System.Collections.Specialized;

namespace Orchard.OutputCache.Services {

    [OrchardFeature("Orchard.OutputCache2")]
    public class OutputCacheOwinMiddleware : IOwinMiddlewareProvider {
        private readonly IWorkContextAccessor _wca;
        private readonly Work<ICacheManager> _cacheManager;
        private readonly Work<IAsyncTagCache> _tagCache;
        private readonly Work<IAsyncOutputCacheStorageProvider> _cacheStorageProvider;
        //private readonly Work<IDisplayedContentItemHandler> _displayedContentItemHandler;
        private readonly Work<IWorkContextAccessor> _workContextAccessor;
        private readonly Work<IThemeManager> _themeManager;
        private readonly Work<IClock> _clock;
        //private readonly Work<ICacheService> _cacheService;
        private readonly Work<ISignals> _signals;
        private readonly Work<ShellSettings> _shellSettings;

        private static string _refreshKey = "__r";
        private static long _epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;

        public OutputCacheOwinMiddleware(
            IWorkContextAccessor wca,
            Work<ICacheManager> cacheManager,
            Work<IAsyncOutputCacheStorageProvider> cacheStorageProvider,
            Work<IAsyncTagCache> tagCache,
            //Work<IDisplayedContentItemHandler> displayedContentItemHandler,
            Work<IWorkContextAccessor> workContextAccessor,
            Work<IThemeManager> themeManager,
            Work<IClock> clock,
            //Work<ICacheService> cacheService,
            Work<ISignals> signals,
            Work<ShellSettings> shellSettings) {

            _cacheManager = cacheManager;
            _cacheStorageProvider = cacheStorageProvider;
            _tagCache = tagCache;
            //_displayedContentItemHandler = displayedContentItemHandler;
            _workContextAccessor = workContextAccessor;
            _themeManager = themeManager;
            _clock = clock;
            //_cacheService = cacheService;
            _signals = signals;
            _shellSettings = shellSettings;

            _wca = wca;
            
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares() {
            return new[]
            {
                // Although we only construct a single OwinMiddlewareRegistration here, you could return multiple ones of course.
                new OwinMiddlewareRegistration
                {
                    // The priority value decides the order of OwinMiddlewareRegistrations. I.e. "0" will run before "10", but registrations
                    // without a priority value will run before the ones that have it set.
                    // Note that this priority notation is the same as the one for shape placement (so you can e.g. use ":before").
                    Priority = "0",

                    // This is the delegate that sets up middlewares.
                    Configure = app =>

                    app.Use( async (context, next) => {

                        var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;
                        var workContext = _wca.GetContext(httpContext);

                        var cacheManager = new DefaultCacheManager(typeof(OutputCacheOwinMiddleware), workContext.Resolve<ICacheHolder>()); // _cacheManager.Value;
                        var cacheStorageProvider = _cacheStorageProvider.Value;
                        //var displayedContentItemHandler = _displayedContentItemHandler.Value;
                        var themeManager = _themeManager.Value;
                        var clock = _clock.Value;
                        //var cacheService = _cacheService.Value;
                        var signals = _signals.Value;
                        var tagCache = _tagCache.Value;
                        var shellSettings = _shellSettings.Value;

                        var cacheSettings = cacheManager.Get(CacheSettings.CacheKey, ctx => {
                            ctx.Monitor(signals.When(CacheSettings.CacheKey));
                            // return new CacheSettings(workContext.CurrentSite.As<CacheSettingsPart>());
                            return new CacheSettings();
                        });

                        // State.
                        DateTime now;
                        string cacheKey;
                        string invariantCacheKey;
                        bool transformRedirect;
                        bool isCachingRequest;
                        bool isCacheableRequest;

                        #region OnActionExecuting
                        Logger.Debug("Incoming request for URL '{0}'.", httpContext.Request.RawUrl);

                        // This filter is not reentrant (multiple executions within the same request are
                        // not supported) so child actions are ignored completely.
                        //if (filterContext.IsChildAction) {
                        //    Logger.Debug("Action '{0}' ignored because it's a child action.", filterContext.ActionDescriptor.ActionName);
                        //    return;
                        //}

                        now = clock.UtcNow;

                        isCacheableRequest = RequestIsCacheable(context, workContext, cacheSettings);

                        // if the request can't be cached, process normally
                        if(!isCacheableRequest) {
                            await next.Invoke();
                            return;
                        }

                        // Computing the cache key after we know that the request is cacheable means that we are only performing this calculation on requests that require it
                        cacheKey = ComputeCacheKey(context, shellSettings, GetCacheKeyParameters(context, themeManager, workContext, cacheSettings));
                        invariantCacheKey = ComputeCacheKey(context, shellSettings, null);

                        Logger.Debug("Cache key '{0}' was created.", cacheKey);


                        try {

                            // Is there a cached item, and are we allowed to serve it?
                            var allowServeFromCache = httpContext.Request.Headers["Cache-Control"] != "no-cache" || cacheSettings.IgnoreNoCache;
                            var cacheItem = await GetCacheItemAsync(cacheKey, cacheStorageProvider);
                            if (allowServeFromCache && cacheItem != null) {

                                Logger.Debug("Item '{0}' was found in cache.", cacheKey);

                                // Is the cached item in its grace period?
                                if (cacheItem.IsInGracePeriod(now)) {

                                    // Render the content unless another request is already doing so.
                                    if (Monitor.TryEnter(String.Intern(cacheKey))) {
                                        Logger.Debug("Item '{0}' is in grace period and not currently being rendered; rendering item...", cacheKey);
                                        BeginRenderItem(context, cacheSettings, out isCachingRequest);
                                        return;
                                    }
                                }

                                // Cached item is not yet in its grace period, or is already being
                                // rendered by another request; serve it from cache.
                                Logger.Debug("Serving item '{0}' from cache.", cacheKey);
                                await ServeCachedItemAsync(context, cacheSettings, cacheItem);
                                return;
                            }

                            // No cached item found, or client doesn't want it; acquire the cache key
                            // lock to render the item.
                            Logger.Debug("Item '{0}' was not found in cache or client refuses it. Acquiring cache key lock...", cacheKey);
                            if (Monitor.TryEnter(String.Intern(cacheKey), TimeSpan.FromSeconds(20))) {
                                Logger.Debug("Cache key lock for item '{0}' was acquired.", cacheKey);

                                // Item might now have been rendered and cached by another request; if so serve it from cache.
                                if (allowServeFromCache) {
                                    cacheItem = await GetCacheItemAsync(cacheKey, cacheStorageProvider);
                                    if (cacheItem != null) {
                                        Logger.Debug("Item '{0}' was now found; releasing cache key lock and serving from cache.", cacheKey);
                                        Monitor.Exit(String.Intern(cacheKey));
                                        await ServeCachedItemAsync(context, cacheSettings, cacheItem);
                                        return;
                                    }
                                }
                            }

                            // Either we acquired the cache key lock and the item was still not in cache, or
                            // the lock acquisition timed out. In either case render the item.
                            Logger.Debug("Rendering item '{0}'...", cacheKey);
                            BeginRenderItem(context, cacheSettings, out isCachingRequest);

                        }
                        catch {
                            // Remember to release the cache key lock in the event of an exception!
                            Logger.Debug("Exception occurred for item '{0}'; releasing any acquired lock.", cacheKey);
                            if (Monitor.IsEntered(String.Intern(cacheKey)))
                                Monitor.Exit(String.Intern(cacheKey));
                            throw;
                        }
                        #endregion


                        //var response = httpContext.Response;
                        //var captureStream = new CaptureStream(response.Filter);
                        //response.Filter = captureStream;
                        //captureStream.Captured += (output) => {
                        //    cacheItem = new CacheItem() {
                        //        Output = output,
                        //        ContentType = response.ContentType,
                        //        StatusCode = response.StatusCode,
                        //        Duration = 300
                        //    };

                        //    using (var scope = _wca.CreateWorkContextScope()) {
                        //        var appDataFolder = scope.Resolve<IAppDataFolder>();
                        //        var shellSettings = scope.Resolve<ShellSettings>();
                        //        var filename = appDataFolder.Combine("OutputCache", shellSettings.Name, HttpUtility.UrlEncode(url));
                        //        using (var stream = Serialize(cacheItem)) {
                        //            using (var fileStream = appDataFolder.CreateFile(filename)) {
                        //                stream.CopyToAsync(fileStream);
                        //            }
                        //        }
                        //    }
                        //};

                        await next.Invoke();

                        transformRedirect = await TransformRedirectAsync(context, cacheSettings, shellSettings, now, tagCache, cacheStorageProvider);

                        #region OnResultExecuted
                        var captureHandlerIsAttached = false;

                        try {

                            // This filter is not reentrant (multiple executions within the same request are
                            // not supported) so child actions are ignored completely.
                            //if (filterContext.IsChildAction )
                            //    return;

                            if(!isCachingRequest) {
                                return;
                            }

                            Logger.Debug("Item '{0}' was rendered.", cacheKey);

                            // Obtain individual route configuration, if any.
                            //CacheRouteConfig configuration = null;
                            //var configurations = cacheService.GetRouteConfigs();
                            //if (configurations.Any()) {
                            //    var route = filterContext.Controller.ControllerContext.RouteData.Route;
                            //    var key = _cacheService.GetRouteDescriptorKey(filterContext.HttpContext, route);
                            //    configuration = configurations.FirstOrDefault(c => c.RouteKey == key);
                            //}

                            //if (!ResponseIsCacheable(filterContext, configuration)) {
                            //    filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                            //    filterContext.HttpContext.Response.Cache.SetNoStore();
                            //    filterContext.HttpContext.Response.Cache.SetMaxAge(new TimeSpan(0));
                            //    return;
                            //}

                            // Determine duration and grace time.
                            //var cacheDuration = configuration != null && configuration.Duration.HasValue ? configuration.Duration.Value : CacheSettings.DefaultCacheDuration;
                            //var cacheGraceTime = configuration != null && configuration.GraceTime.HasValue ? configuration.GraceTime.Value : CacheSettings.DefaultCacheGraceTime;
                            
                            // TODO: adapt 
                            var cacheDuration = cacheSettings.DefaultCacheDuration;
                            var cacheGraceTime = cacheSettings.DefaultCacheGraceTime;


                            //// Include each content item ID as tags for the cache entry.
                            //var contentItemIds = _displayedContentItemHandler.GetDisplayed().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();

                            //// Capture the response output using a custom filter stream.
                            //var response = filterContext.HttpContext.Response;

                            var response = httpContext.Response;
                            var captureStream = new CaptureStream(response.Filter);
                            response.Filter = captureStream;
                            captureStream.Captured += (output) => {
                                try {
                                    // Since this is a callback any call to injected dependencies can result in an Autofac exception: "Instances 
                                    // cannot be resolved and nested lifetimes cannot be created from this LifetimeScope as it has already been disposed."
                                    // To prevent access to the original lifetime scope a new work context scope should be created here and dependencies
                                    // should be resolved from it.

                                    using (var scope = _wca.CreateWorkContextScope()) {
                                        var cacheItem = new CacheItem() {
                                            CachedOnUtc = now,
                                            Duration = cacheDuration,
                                            GraceTime = cacheGraceTime,
                                            Output = output,
                                            ContentType = response.ContentType,
                                            QueryString = httpContext.Request.Url.Query,
                                            CacheKey = cacheKey,
                                            InvariantCacheKey = invariantCacheKey,
                                            Url = httpContext.Request.Url.AbsolutePath,
                                            Tenant = scope.Resolve<ShellSettings>().Name,
                                            StatusCode = response.StatusCode,
                                            Tags = new[] { invariantCacheKey } //.Union(contentItemIds).ToArray()
                                        };

                                        // Write the rendered item to the cache.
                                        var localCacheStorageProvider = scope.Resolve<IAsyncOutputCacheStorageProvider>();
                                        cacheStorageProvider.RemoveAsync(cacheKey);
                                        cacheStorageProvider.SetAsync(cacheKey, cacheItem);

                                        Logger.Debug("Item '{0}' was written to cache.", cacheKey);

                                        // Also add the item tags to the tag cache.
                                        var localTagCache = scope.Resolve<IAsyncTagCache>();
                                        foreach (var tag in cacheItem.Tags) {
                                            localTagCache.TagAsync(tag, cacheKey);
                                        }
                                    }
                                }
                                finally {
                                    // Always release the cache key lock when the request ends.
                                    ReleaseCacheKeyLock(cacheKey);
                                }
                            };

                            captureHandlerIsAttached = true;
                        }
                        finally {
                            // If the response filter stream capture handler was attached then we'll trust
                            // it to release the cache key lock at some point in the future when the stream
                            // is flushed; otherwise we'll make sure we'll release it here.
                            if (!captureHandlerIsAttached)
                                ReleaseCacheKeyLock(String.Intern(cacheKey));
                        }
                        #endregion

                    })
                }
            };
        }

        protected virtual bool RequestIsCacheable(IOwinContext context, WorkContext workContext, CacheSettings cacheSettings) {

            var itemDescriptor = string.Empty;
            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;

            //if (Logger.IsEnabled(LogLevel.Debug)) {
            //    var url = filterContext.RequestContext.HttpContext.Request.RawUrl;
            //    var area = filterContext.RequestContext.RouteData.Values["area"];
            //    var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            //    var action = filterContext.ActionDescriptor.ActionName;
            //    var culture = _workContext.CurrentCulture.ToLowerInvariant();
            //    var auth = filterContext.HttpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant();
            //    var theme = _themeManager.GetRequestTheme(filterContext.RequestContext).Id.ToLowerInvariant();

            //    itemDescriptor = string.Format("{0} (Area: {1}, Controller: {2}, Action: {3}, Culture: {4}, Theme: {5}, Auth: {6})", url, area, controller, action, culture, theme, auth);
            //}

            // Respect OutputCacheAttribute if applied.
            //var actionAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            //var controllerAttributes = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            //var outputCacheAttribute = actionAttributes.Concat(controllerAttributes).Cast<OutputCacheAttribute>().FirstOrDefault();
            //if (outputCacheAttribute != null) {
            //    if (outputCacheAttribute.Duration <= 0 || outputCacheAttribute.NoStore) {
            //        Logger.Debug("Request for item '{0}' ignored based on OutputCache attribute.", itemDescriptor);
            //        return false;
            //    }
            //}

            // Don't cache POST requests.
            if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                Logger.Debug("Request for item '{0}' ignored because HTTP method is POST.", itemDescriptor);
                return false;
            }

            // Don't cache admin section requests.
            if (AdminFilter.IsApplied(new RequestContext(httpContext, new RouteData()))) {
                Logger.Debug("Request for item '{0}' ignored because it's in admin section.", itemDescriptor);
                return false;
            }

            // Ignore authenticated requests unless the setting to cache them is true.
            if (workContext.CurrentUser != null && !cacheSettings.CacheAuthenticatedRequests) {
                Logger.Debug("Request for item '{0}' ignored because user is authenticated.", itemDescriptor);
                return false;
            }

            // Don't cache ignored URLs.
            if (IsIgnoredUrl(httpContext.Request.AppRelativeCurrentExecutionFilePath, cacheSettings.IgnoredUrls)) {
                Logger.Debug("Request for item '{0}' ignored because the URL is configured as ignored.", itemDescriptor);
                return false;
            }

            // Ignore requests with the refresh key on the query string.
            foreach (var key in httpContext.Request.QueryString.AllKeys) {
                if (String.Equals(_refreshKey, key, StringComparison.OrdinalIgnoreCase)) {
                    Logger.Debug("Request for item '{0}' ignored because refresh key was found on query string.", itemDescriptor);
                    return false;
                }
            }

            return true;
        }

        protected virtual bool ResponseIsCacheable(IOwinContext context, CacheRouteConfig configuration, bool transformRedirect, string cacheKey) {

            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;

            if (httpContext.Request.Url == null) {
                return false;
            }

            // Don't cache non-200 responses or results of a redirect.
            var response = httpContext.Response;
            if (response.StatusCode != 200 || transformRedirect) {
                return false;
            }

            // Don't cache in individual route configuration says no.
            if (configuration != null && configuration.Duration == 0) {
                Logger.Debug("Response for item '{0}' will not be cached because route is configured to not be cached.", cacheKey);
                return false;
            }

            // Don't cache if request created notifications.
            //var hasNotifications = !String.IsNullOrEmpty(Convert.ToString(filterContext.Controller.TempData["messages"]));
            //if (hasNotifications) {
            //    Logger.Debug("Response for item '{0}' will not be cached because one or more notifications were created.", _cacheKey);
            //    return false;
            //}

            return true;
        }

        protected virtual IDictionary<string, object> GetCacheKeyParameters(IOwinContext context, IThemeManager themeManager, WorkContext workContext, CacheSettings cacheSettings) {
            var result = new Dictionary<string, object>();

            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;

            // Vary by action parameters.
            //foreach (var p in filterContext.ActionParameters)
            //    result.Add("PARAM:" + p.Key, p.Value);

            // Vary by theme.
            result.Add("theme", themeManager.GetRequestTheme(httpContext.Request.RequestContext).Id.ToLowerInvariant());

            // Vary by configured query string parameters.
            var queryString = httpContext.Request.RequestContext.HttpContext.Request.QueryString;
            foreach (var key in queryString.AllKeys) {
                if (key == null || (cacheSettings.VaryByQueryStringParameters != null && !cacheSettings.VaryByQueryStringParameters.Contains(key)))
                    continue;
                result[key] = queryString[key];
            }

            // Vary by configured request headers.
            var requestHeaders = httpContext.Request.RequestContext.HttpContext.Request.Headers;
            foreach (var varyByRequestHeader in cacheSettings.VaryByRequestHeaders) {
                if (requestHeaders.AllKeys.Contains(varyByRequestHeader))
                    result["HEADER:" + varyByRequestHeader] = requestHeaders[varyByRequestHeader];
            }


            // Vary by request culture if configured.
            if (cacheSettings.VaryByCulture) {
                result["culture"] = workContext.CurrentCulture.ToLowerInvariant();
            }

            // Vary by authentication state if configured.
            if (cacheSettings.VaryByAuthenticationState) {
                result["auth"] = httpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant();
            }

            return result;
        }

        protected virtual async Task<bool> TransformRedirectAsync(IOwinContext context, CacheSettings cacheSettings, ShellSettings shellSettings, DateTime now, IAsyncTagCache tagCache, IAsyncOutputCacheStorageProvider cacheStorageProvider) {

            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;

            // Removes the target of the redirection from cache after a POST.

            //if (filterContext.Result == null) {
            //    throw new ArgumentNullException();
            //}

            if (AdminFilter.IsApplied(new RequestContext(httpContext, new RouteData()))) {
                return false;
            }

            RedirectResult redirectResult = null; // filterContext.Result as RedirectResult;

            // status code can't be tested at this point, so test the result type instead
            if (redirectResult == null || !httpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            Logger.Debug("Redirect on POST detected; removing from cache and adding refresh key.");

            var redirectUrl = redirectResult.Url;

            if (httpContext.Request.IsLocalUrl(redirectUrl)) {
                // Remove all cached versions of the same item.
                var helper = new UrlHelper(httpContext.Request.RequestContext);
                var absolutePath = new Uri(helper.MakeAbsolute(redirectUrl)).AbsolutePath;
                var invariantCacheKey = ComputeCacheKey(shellSettings.Name, absolutePath, null);

                foreach (var key in await tagCache.GetTaggedItemsAsync(invariantCacheKey)) {
                    await cacheStorageProvider.RemoveAsync(key);
                }

                // we no longer need the tag entry as the items have been removed
                await tagCache.RemoveTagAsync(invariantCacheKey);

            }

            // Adding a refresh key so that the redirection doesn't get restored
            // from a cached version on a proxy. This can happen when using public
            // caching, we want to force the client to get a fresh copy of the
            // redirectUrl content.

            if (cacheSettings.DefaultMaxAge > 0) {
                var epIndex = redirectUrl.IndexOf('?');
                var qs = new NameValueCollection();
                if (epIndex > 0) {
                    qs = HttpUtility.ParseQueryString(redirectUrl.Substring(epIndex));
                }

                // Substract Epoch to get a smaller number.
                var refresh = now.Ticks - _epoch;
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

            //filterContext.Result = new RedirectResult(redirectUrl, redirectResult.Permanent);

            //filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            return true;
        }
        
        private async Task ServeCachedItemAsync(IOwinContext context, CacheSettings cacheSettings, CacheItem cacheItem) {
            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;

            // Fix for missing charset in response headers
            // context.Response.Charset = response.Charset;

            // Adds some caching information to the output if requested.
            if (cacheSettings.DebugMode) {
                //response.AddHeader("X-Cached-On", cacheItem.CachedOnUtc.ToString("r"));
                //response.AddHeader("X-Cached-Until", cacheItem.ValidUntilUtc.ToString("r"));
            }

            context.Response.ContentType = cacheItem.ContentType;

            await context.Response.WriteAsync(cacheItem.Output);


            context.Response.StatusCode = cacheItem.StatusCode;

            ApplyCacheControl(context, cacheSettings);
        }

        private void BeginRenderItem(IOwinContext context, CacheSettings cacheSettings, out bool isCachingRequest) {
            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;

            var response = httpContext.Response;

            ApplyCacheControl(context, cacheSettings);

            // Remember that we should intercept the rendered response output.
            isCachingRequest = true;
        }

        private void ApplyCacheControl(IOwinContext context, CacheSettings cacheSettings) {

            if (cacheSettings.DefaultMaxAge > 0) {
                var maxAge = TimeSpan.FromSeconds(cacheSettings.DefaultMaxAge); //cacheItem.ValidUntilUtc - _clock.UtcNow;
                if (maxAge.TotalMilliseconds < 0) {
                    maxAge = TimeSpan.Zero;
                }
                //response.Cache.SetCacheability(HttpCacheability.Public);
                //response.Cache.SetMaxAge(maxAge);
            }

            // Keeping this example for later usage.
            // response.DisableUserCache();
            // response.DisableKernelCache();
            // response.Cache.SetOmitVaryStar(true);

            // An ETag is a string that uniquely identifies a specific version of a component.
            // We use the cache item to detect if it's a new one.
            if (HttpRuntime.UsingIntegratedPipeline) {
                //if (response.Headers.Get("ETag") == null) {
                //    // What is the point of GetHashCode() of a newly generated item? /DanielStolt
                //    response.Cache.SetETag(new CacheItem().GetHashCode().ToString(CultureInfo.InvariantCulture));
                //}
            }

            //if (cacheSettings.VaryByQueryStringParameters == null) {
            //    response.Cache.VaryByParams["*"] = true;
            //}
            //else {
            //    foreach (var queryStringParam in cacheSettings.VaryByQueryStringParameters) {
            //        response.Cache.VaryByParams[queryStringParam] = true;
            //    }
            //}

            //foreach (var varyRequestHeader in cacheSettings.VaryByRequestHeaders) {
            //    response.Cache.VaryByHeaders[varyRequestHeader] = true;
            //}
        }

        private void ReleaseCacheKeyLock(string cacheKey) {
            if (cacheKey != null) {
                if (Monitor.IsEntered(String.Intern(cacheKey))) {
                    Logger.Debug("Releasing cache key lock for item '{0}'.", cacheKey);
                    Monitor.Exit(String.Intern(cacheKey));
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

        protected virtual string ComputeCacheKey(IOwinContext context, ShellSettings shellSettings, IEnumerable<KeyValuePair<string, object>> parameters) {
            var httpContext = context.Environment["System.Web.HttpContextBase"] as HttpContextBase;
            var url = httpContext.Request.Url.AbsolutePath;
            return ComputeCacheKey(shellSettings.Name, url, parameters);
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

        protected virtual async Task<CacheItem> GetCacheItemAsync(string key, IAsyncOutputCacheStorageProvider cacheStorageProvider) {
            try {
                var cacheItem = await cacheStorageProvider.GetAsync(key);
                return cacheItem;
            }
            catch (Exception e) {
                Logger.Error(e, "An unexpected error occured while reading a cache entry");
            }

            return null;
        }

    }
}