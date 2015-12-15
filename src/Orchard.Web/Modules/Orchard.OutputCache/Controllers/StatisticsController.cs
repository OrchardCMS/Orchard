using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.OutputCache.Services;
using Orchard.OutputCache.ViewModels;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Orchard.OutputCache.Controllers {
    [Admin]
    public class StatisticsController : Controller {
        private readonly ICacheService _cacheService;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly ShellSettings _shellSettings;
        private readonly ISiteService _siteService;

        public StatisticsController(
            IOrchardServices services,
            ICacheService cacheService,
            IOutputCacheStorageProvider cacheStorageProvider,
            ShellSettings shellSettings,
            ISiteService siteService) {
            _cacheService = cacheService;
            _cacheStorageProvider = cacheStorageProvider;
            _shellSettings = shellSettings;
            _siteService = siteService;
            Services = services;
            }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You do not have permission to manage output cache.")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = Services.New.Pager(pager).TotalItemCount(_cacheStorageProvider.GetCacheItemsCount());

            var model = new StatisticsViewModel {
                CacheItems = _cacheStorageProvider
                    .GetCacheItems(pager.GetStartIndex(), pager.PageSize)
                    .ToList(),
                Pager = pagerShape
            };

            return View(model);
        }

        public ActionResult Evict(string cacheKey) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You do not have permission to manage output cache.")))
                return new HttpUnauthorizedResult();

            _cacheStorageProvider.Remove(cacheKey);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EvictAll() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You do not have permission to manage output cache.")))
                return new HttpUnauthorizedResult();

            _cacheStorageProvider.RemoveAll();

            return RedirectToAction("Index");
        }
    }
}