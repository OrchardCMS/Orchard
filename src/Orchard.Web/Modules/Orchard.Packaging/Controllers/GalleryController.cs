using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.Events;
using Orchard.Packaging.Extensions;
using Orchard.Packaging.GalleryServer;
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
using NullLogger = Orchard.Logging.NullLogger;

namespace Orchard.Packaging.Controllers {
    [OrchardFeature("Gallery")]
    [Themed, Admin]
    public class GalleryController : Controller {
        private readonly ShellSettings _shellSettings;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IExtensionDisplayEventHandler _extensionDisplayEventHandler;
        private readonly IExtensionManager _extensionManager;

        public GalleryController(
            IEnumerable<IExtensionDisplayEventHandler> extensionDisplayEventHandlers,
            ShellSettings shellSettings,
            IPackagingSourceManager packagingSourceManager,
            IExtensionManager extensionManager,
            IOrchardServices services) {

            _shellSettings = shellSettings;
            _packagingSourceManager = packagingSourceManager;
            _extensionDisplayEventHandler = extensionDisplayEventHandlers.FirstOrDefault();
            _extensionManager = extensionManager;
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

        [HttpPost]
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
            } 
            catch {
                ModelState.AddModelError("Url", T("The url of the feed or its content is not valid.").Text);
            }

            if (!ModelState.IsValid)
                return View(new PackagingAddSourceViewModel { Url = url });

            _packagingSourceManager.AddSource(title, url);
            Services.Notifier.Information(T("The feed has been added successfully."));

            return RedirectToAction("Sources");
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
                    Expression<Func<PublishedPackage, bool>> packagesCriteria = p => p.PackageType == packageType &&
                                p.IsLatestVersion &&
                                (string.IsNullOrEmpty(options.SearchText)
                                    || p.Title.Contains(options.SearchText)
                                    || p.Description.Contains(options.SearchText)
                                    || p.Tags.Contains(options.SearchText));

                    var sourceExtensions = _packagingSourceManager.GetExtensionList(true,
                        source,
                        packages => {
                            packages = packages.Where(packagesCriteria);

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

                            if (pager.PageSize != 0) {
                                packages = packages.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize);
                            }

                            return packages;
                        }).ToArray();

                    // count packages separately to prevent loading everything just to count
                    totalCount += _packagingSourceManager.GetExtensionCount(
                        source,
                        packages => packages.Where(packagesCriteria));

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
                } 
                catch (Exception exception) {
                    Services.Notifier.Error(T("Error loading extensions from gallery source '{0}'. {1}.", source.FeedTitle, exception.Message));
                }
            }

            extensions = extensions ?? new PackagingEntry[0];
            var pagerShape = Services.New.Pager(pager).TotalItemCount(totalCount);

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Order", options.Order);
            routeData.Values.Add("Options.SearchText", options.SearchText);
            routeData.Values.Add("Options.SourceId", options.SourceId);
            pagerShape.RouteData(routeData);

            extensions = extensions.ToList();

            // Populate the notifications
            IEnumerable<Tuple<ExtensionDescriptor, PackagingEntry>> extensionDescriptors = _extensionManager.AvailableExtensions()
                .Join(extensions, extensionDescriptor => extensionDescriptor.Id, packaginEntry => packaginEntry.ExtensionId(),
                      (extensionDescriptor, packagingEntry) => new Tuple<ExtensionDescriptor, PackagingEntry>(extensionDescriptor, packagingEntry));

            foreach (Tuple<ExtensionDescriptor, PackagingEntry> packagings in extensionDescriptors) {
                packagings.Item2.Installed = true;

                if (_extensionDisplayEventHandler != null) {
                    foreach (string notification in _extensionDisplayEventHandler.Displaying(packagings.Item1, ControllerContext.RequestContext)) {
                        packagings.Item2.Notifications.Add(notification);
                    }
                }
            }

            return View(packageType == DefaultExtensionTypes.Theme ? "Themes" : "Modules", new PackagingExtensionsViewModel {
                Extensions = extensions,
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                Pager = pagerShape,
                Options = options
            });
        }
    }
}