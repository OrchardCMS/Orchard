using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.ViewModels;
using Orchard.Core.Contents.Controllers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Alias.Controllers {
    [OrchardFeature("Orchard.Alias.UI")]
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly IAliasService _aliasService;
        private readonly IAliasHolder _aliasHolder;

        public AdminController(
            IAliasService aliasService,
            IOrchardServices orchardServices,
            IAliasHolder aliasHolder) {
            _aliasService = aliasService;
            _aliasHolder = aliasHolder;
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public ActionResult Index(AdminIndexOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);

            // default options
            if (options == null)
                options = new AdminIndexOptions();

            switch (options.Filter) {
                case AliasFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var aliases = _aliasHolder.GetMaps().SelectMany(x => x.GetAliases());

            if (!String.IsNullOrWhiteSpace(options.Search)) {
                var invariantSearch = options.Search.ToLowerInvariant();
                aliases = aliases.Where(x => x.Path.ToLowerInvariant().Contains(invariantSearch));
            }

            aliases = aliases.ToList();

            var pagerShape = Services.New.Pager(pager).TotalItemCount(aliases.Count());

            switch (options.Order) {
                case AliasOrder.Path:
                    aliases = aliases.OrderBy(x => x.Path);
                    break;
            }

            if (pager.PageSize != 0) {
                aliases = aliases.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            }

            var model = new AdminIndexViewModel {
                Options = options,
                Pager = pagerShape,
                AliasEntries = aliases.Select(x => new AliasEntry() { Alias = x, IsChecked = false }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdminIndexViewModel { AliasEntries = new List<AliasEntry>(), Options = new AdminIndexOptions() };
            UpdateModel(viewModel);

            var checkedItems = viewModel.AliasEntries.Where(c => c.IsChecked);

            switch (viewModel.Options.BulkAction) {
                case AliasBulkAction.None:
                    break;
                case AliasBulkAction.Delete:
                    foreach (var checkedItem in checkedItems) {
                        _aliasService.Delete(checkedItem.Alias.Path);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Add() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            return View();
        }

        [HttpPost]
        public ActionResult Add(string aliasPath, string routePath) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            if (aliasPath == "/") {
                aliasPath = String.Empty;
            }

            if (String.IsNullOrWhiteSpace(aliasPath)) {
                ModelState.AddModelError("Path", T("Path can't be empty").Text);
            }

            if (String.IsNullOrWhiteSpace(aliasPath)) {
                ModelState.AddModelError("Route", T("Route can't be empty").Text);
            }

            if (!ModelState.IsValid) {
                return View();
            }

            try {
                _aliasService.Set(aliasPath, routePath, String.Empty);
            }
            catch {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("An error occured while creating the alias. Please check the values are correct.", aliasPath));

                ViewBag.Path = aliasPath;
                ViewBag.Route = routePath;

                return View();
            }

            Services.Notifier.Information(T("Alias {0} created", aliasPath));

            return RedirectToAction("Index");
        }

        public ActionResult Edit(string path) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            if (path == "/") {
                path = String.Empty;
            }

            var routeValues = _aliasService.Get(path);

            if (routeValues == null)
                return HttpNotFound();

            var virtualPaths = _aliasService.LookupVirtualPaths(routeValues, HttpContext)
                .Select(vpd => vpd.VirtualPath);

            ViewBag.AliasPath = path;
            ViewBag.RoutePath = virtualPaths.FirstOrDefault();

            return View("Edit");
        }

        [HttpPost]
        public ActionResult Edit(string path, string aliasPath, string routePath) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            // TODO: (PH:Autoroute) This could overwrite an existing Alias without warning, should handle this
            _aliasService.Set(aliasPath, routePath, "Custom");

            // Remove previous alias
            if (path != aliasPath) {
                // TODO: (PH:Autoroute) Ability to fire an "AliasChanged" event so we make a redirect
                _aliasService.Delete(path);
            }

            Services.Notifier.Information(T("Alias {0} updated", path));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(string path) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            if (path == "/") {
                path = String.Empty;
            }

            _aliasService.Delete(path);

            Services.Notifier.Information(T("Alias {0} deleted", path));

            return RedirectToAction("Index");
        }
    }
}