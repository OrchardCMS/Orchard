using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Autofac.Features.Metadata;
using Orchard;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;
using Orchard.Mvc;
using Orchard.Services;
using Orchard.Settings;
using Orchard.UI.Resources;

namespace Fcm.ResourceTimestamper
{
    [OrchardSuppressDependency("Orchard.UI.Resources.ResourceManager")]
    public class FcmResourceManager : ResourceManager
    {
        public const string DataTimestampedAttributeKey = "data-timestamped";
        private readonly IClock _clock;
        private readonly ICacheManager _cacheManager;
        private readonly IWebSiteFolder _websiteFolder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Work<WorkContext> _workContext;

        public FcmResourceManager(IEnumerable<Meta<IResourceManifestProvider>> resourceProviders,
            IClock clock, ICacheManager cacheManager, IWebSiteFolder websiteFolder, IHttpContextAccessor httpContextAccessor,
            Work<WorkContext> workContext)
            : base(resourceProviders)
        {
            _clock = clock;
            _cacheManager = cacheManager;
            _websiteFolder = websiteFolder;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
        }

        public override IList<ResourceRequiredContext> BuildRequiredResources(string stringResourceType)
        {
            var resources = new List<ResourceRequiredContext>(base.BuildRequiredResources(stringResourceType));

            foreach (var resource in resources) {
                if (resource.Settings.Attributes == null)
                    continue;

                var attributes = resource.Settings.Attributes.ToList();
                foreach (var attr in attributes) {
                    if (attr.Key == DataTimestampedAttributeKey) {
                        var cachedUrl = GetCachedUrl(resource);
                        var cachedUrlDebug = GetCachedUrl(resource);
                        resource.Resource.SetUrl(cachedUrl);
                    }
                }
            }
            return resources;
        }

        private string GetCachedUrl(ResourceRequiredContext resource)
        {
            if (resource.Resource.Url.StartsWith("http://") || resource.Resource.Url.StartsWith("https://")) 
            {
                return resource.Resource.Url;
            }

            string path = GetResourcePath(resource);

            // it's a local file:
            return _cacheManager.Get("Fcm.ResourceCache." + path, ctx =>
            {
                ctx.Monitor(_websiteFolder.WhenPathChanges(path));
                return GetUpdatedUrl(path);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private string GetResourcePath(ResourceRequiredContext resource)
        {
            bool debugMode;
            var site = _workContext.Value.CurrentSite;
            switch (site.ResourceDebugMode)
            {
                case ResourceDebugMode.Enabled:
                    debugMode = true;
                    break;
                case ResourceDebugMode.Disabled:
                    debugMode = false;
                    break;
                default:
                    debugMode = _httpContextAccessor.Current().IsDebuggingEnabled;
                    break;
            }
            var defaultSettings = new RequireSettings
            {
                DebugMode = debugMode,
                Culture = CultureInfo.CurrentUICulture.Name,
            };
            var appPath = _httpContextAccessor.Current().Request.ApplicationPath;
            var path = resource.GetResourceUrl(defaultSettings, appPath);
            return path;
        }

        private string GetUpdatedUrl(string path)
        {
            HttpRequestBase request = _httpContextAccessor.Current().Request;

            string convertedUrl = string.Format("http{0}://{1}{2}{3}", request.IsSecureConnection ? "s" : "",
                request.Url.Host, request.Url.IsDefaultPort ? "" : ":" + request.Url.Port,
                    VirtualPathUtility.ToAbsolute(path));

            // sanity check:
            const string timestampFragment = "?v=";
            string ticks = _clock.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
                
            if (convertedUrl.Contains(timestampFragment))
            {
                convertedUrl = convertedUrl.Substring(0, convertedUrl.IndexOf(timestampFragment, StringComparison.Ordinal));
            }

            return convertedUrl + "?v=" + ticks;
        }
    }
}
