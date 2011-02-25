using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using NuGet;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Packaging.ViewModels;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using ILogger = Orchard.Logging.ILogger;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;
using NullLogger = Orchard.Logging.NullLogger;

namespace Orchard.Packaging.Controllers {
    [OrchardFeature("Gallery")]
    [Themed, Admin]
    public class GalleryController : Controller {
        private readonly ShellSettings _shellSettings;
        private readonly IPackageManager _packageManager;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IFeatureManager _featureManager;

        public GalleryController(
            ShellSettings shellSettings,
            IPackageManager packageManager,
            IPackagingSourceManager packagingSourceManager,
            IFeatureManager featureManager,
            IOrchardServices services) {
            _shellSettings = shellSettings;
            _packageManager = packageManager;
            _packagingSourceManager = packagingSourceManager;
            _featureManager = featureManager;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Sources() {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list sources")))
                return new HttpUnauthorizedResult();

            return View(new PackagingSourcesViewModel {
                Sources = _packagingSourceManager.GetSources(),
            });
        }

        public ActionResult Remove(int id) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to remove sources")))
                return new HttpUnauthorizedResult();

            _packagingSourceManager.RemoveSource(id);
            Services.Notifier.Information(T("The feed has been removed successfully."));
            return RedirectToAction("Sources");
        }

        public ActionResult AddSource() {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            return View(new PackagingAddSourceViewModel());
        }

        [HttpPost]
        public ActionResult AddSource(string url) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            try {
                if (!String.IsNullOrEmpty(url)) {
                    if (!url.StartsWith("http")) {
                        ModelState.AddModelError("Url", T("The Url is not valid").Text);
                    }
                }
                else if (String.IsNullOrWhiteSpace(url)) {
                    ModelState.AddModelError("Url", T("Url is required").Text);
                }

                string title = null;
                // try to load the feed
                try {

                    XNamespace atomns = "http://www.w3.org/2005/Atom";
                    var feed = XDocument.Load(url, LoadOptions.PreserveWhitespace);
                    var titleNode = feed.Descendants(atomns + "title").FirstOrDefault();
                    if (titleNode != null)
                        title = titleNode.Value;

                    if (String.IsNullOrWhiteSpace(title)) {
                        ModelState.AddModelError("Url", T("The feed has no title.").Text);
                    }
                } catch {
                    ModelState.AddModelError("Url", T("The url of the feed or its content is not valid.").Text);
                }

                if (!ModelState.IsValid)
                    return View(new PackagingAddSourceViewModel { Url = url });

                _packagingSourceManager.AddSource(title, url);
                Services.Notifier.Information(T("The feed has been added successfully."));

                return RedirectToAction("Sources");
            } catch (Exception exception) {
                this.Error(exception, T("Adding feed failed: {0}", exception.Message), Logger, Services.Notifier);

                return View(new PackagingAddSourceViewModel { Url = url });
            }
        }

        public ActionResult Modules(PackagingExtensionsOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list Modules")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);
            return ListExtensions(options, DefaultExtensionTypes.Module, pager);
        }

        public ActionResult Themes(PackagingExtensionsOptions options, PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list Themes")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);
            return ListExtensions(options, DefaultExtensionTypes.Theme, pager);
        }

        protected ActionResult ListExtensions(PackagingExtensionsOptions options, string packageType, Pager pager) {
            var selectedSource = _packagingSourceManager.GetSources().Where(s => s.Id == options.SourceId).FirstOrDefault();

            var sources = selectedSource != null
                ? new[] { selectedSource }
                : _packagingSourceManager.GetSources()
            ;

            IEnumerable<PackagingEntry> extensions = null;
            int totalCount = 0;
            foreach (var source in sources) {
                try {
                    var sourceExtensions = _packagingSourceManager.GetExtensionList(
                        source,
                        packages => {
                            packages = packages.Where(p => p.PackageType == packageType &&
                                p.IsLatestVersion &&
                                (string.IsNullOrEmpty(options.SearchText) || p.Title.Contains(options.SearchText)));

                            switch (options.Order) {
                                case PackagingExtensionsOrder.Downloads:
                                    packages = packages.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Title);
                                    break;
                                case PackagingExtensionsOrder.Ratings:
                                    packages = packages.OrderByDescending(p => p.Rating).ThenBy(p => p.Title);
                                    break;
                                case PackagingExtensionsOrder.Alphanumeric:
                                    packages = packages.OrderBy(p => p.Title);
                                    break;
                            }

                            if(pager.PageSize != 0) {
                                packages = packages.Skip((pager.Page - 1)*pager.PageSize).Take(pager.PageSize);
                            }

                            return packages;
                        }).ToArray();

                    // count packages separately to prevent loading everything just to count
                    totalCount += _packagingSourceManager.GetExtensionCount(
                        source,
                        packages => packages.Where(p => p.PackageType == packageType &&
                                p.IsLatestVersion &&
                                (string.IsNullOrEmpty(options.SearchText) || p.Title.Contains(options.SearchText)))
                        );

                    extensions = extensions == null ? sourceExtensions : extensions.Concat(sourceExtensions);

                    // apply another paging rule in case there were multiple sources
                    if (sources.Count() > 1) {
                        switch (options.Order) {
                            case PackagingExtensionsOrder.Downloads:
                                extensions = extensions.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Title);
                                break;
                            case PackagingExtensionsOrder.Ratings:
                                extensions = extensions.OrderByDescending(p => p.Rating).ThenBy(p => p.Title);
                                break;
                            case PackagingExtensionsOrder.Alphanumeric:
                                extensions = extensions.OrderBy(p => p.Title);
                                break;
                        }

                        if (pager.PageSize != 0) {
                            extensions = extensions.Take(pager.PageSize);
                        }
                    }
                } catch (Exception exception) {
                    this.Error(exception, T("Error loading extensions from gallery source '{0}'. {1}.", source.FeedTitle, exception.Message), Logger, Services.Notifier);
                }
            }

            extensions = extensions ?? new PackagingEntry[0];
            var pagerShape = Services.New.Pager(pager).TotalItemCount(totalCount);

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Order", options.Order);
            routeData.Values.Add("Options.SearchText", options.SearchText);
            pagerShape.RouteData(routeData);

            return View(packageType == DefaultExtensionTypes.Theme ? "Themes" : "Modules", new PackagingExtensionsViewModel {
                Extensions = extensions,
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                SelectedSource = selectedSource,
                Pager = pagerShape,
                Options = options
            });
        }

        public ActionResult Install(string packageId, string version, int sourceId, string redirectTo) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            var source = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();
            if (source == null) {
                return HttpNotFound();
            }

            if (packageId.StartsWith(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme))) {
                return InstallPOST(packageId, version, sourceId, redirectTo);
            }

            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource(source.FeedUrl, "Default"));

            IPackage package = packageRepository.FindPackage(packageId);
            ExtensionDescriptor extensionDescriptor = _packageManager.GetExtensionDescriptor(package);

            List<PackagingInstallFeatureViewModel> features = extensionDescriptor.Features
                .Where(featureDescriptor => !DefaultExtensionTypes.IsTheme(featureDescriptor.Extension.ExtensionType) &&
                    !featureDescriptor.Id.Equals(featureDescriptor.Extension.Id))
                .Select(featureDescriptor => new PackagingInstallFeatureViewModel {
                    Enable = true, // by default all features are enabled
                    FeatureDescriptor = featureDescriptor
                }).ToList();

            return View("InstallModule", new PackagingInstallViewModel {
                Features = features,
                ExtensionDescriptor = extensionDescriptor
            });
        }

        [HttpPost, ActionName("InstallModule")]
        public ActionResult InstallModulePOST(PackagingInstallViewModel packagingInstallViewModel, string packageId, string version, int sourceId, string redirectTo) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            var source = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            if (source == null) {
                return HttpNotFound();
            }

            InstallPackage(packageId, version, source);
            // Enable selected features
            _featureManager.EnableFeatures(packagingInstallViewModel.Features
                                                .Select(feature => feature.FeatureDescriptor.Id));

            return RedirectToAction(redirectTo == "Themes" ? "Themes" : "Modules");
        }

        [HttpPost, ActionName("Install")]
        public ActionResult InstallPOST(string packageId, string version, int sourceId, string redirectTo) {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            var source = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();
            if (source == null) {
                return HttpNotFound();
            }

            InstallPackage(packageId, version, source);

            return RedirectToAction(redirectTo == "Themes" ? "Themes" : "Modules");
        }

        private PackageInfo InstallPackage(string packageId, string version, PackagingSource source) {
            PackageInfo packageInfo = null;
            try {
                packageInfo = _packageManager.Install(packageId, version, source.FeedUrl, HostingEnvironment.MapPath("~/"));

                if (DefaultExtensionTypes.IsTheme(packageInfo.ExtensionType)) {
                    Services.Notifier.Information(T("The theme has been successfully installed. It can be enabled in the \"Themes\" page accessible from the menu."));
                } else if (DefaultExtensionTypes.IsModule(packageInfo.ExtensionType)) {
                    Services.Notifier.Information(T("The module has been successfully installed."));
                }
            } catch (Exception exception) {
                this.Error(exception, T("Package installation failed."), Logger, Services.Notifier);
            }

            return packageInfo;
        }
    }
}