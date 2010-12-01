using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.Packaging.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;

namespace Orchard.Packaging.Controllers {
    [OrchardFeature("Gallery")]
    [Themed, Admin]
    public class GalleryController : Controller {

        private readonly IPackageManager _packageManager;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IAppDataFolderRoot _appDataFolderRoot;
        private readonly INotifier _notifier;

        public GalleryController(
            IPackageManager packageManager,
            IPackagingSourceManager packagingSourceManager,
            INotifier notifier,
            IAppDataFolderRoot appDataFolderRoot) {
            _packageManager = packageManager;
            _packagingSourceManager = packagingSourceManager;
            _notifier = notifier;
            _appDataFolderRoot = appDataFolderRoot;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Sources() {
            return View(new PackagingSourcesViewModel {
                Sources = _packagingSourceManager.GetSources(),
            });
        }

        public ActionResult Remove(int id) {
            _packagingSourceManager.RemoveSource(id);
            _notifier.Information(T("The feed has been removed successfully."));
            return RedirectToAction("Sources");
        }

        public ActionResult AddSource() {
            return View(new PackagingAddSourceViewModel());
        }

        [HttpPost]
        public ActionResult AddSource(string url) {
            try {
                if ( !String.IsNullOrEmpty(url) ) {
                    if (!url.StartsWith("http")) {
                        ModelState.AddModelError("Url", T("The Url is not valid").Text);
                    }
                }
                else if ( String.IsNullOrWhiteSpace(url)) {
                    ModelState.AddModelError("Url", T("Url is required").Text);
                }

                string title = null;
                // try to load the feed
                try {

                    XNamespace atomns = "http://www.w3.org/2005/Atom" ;
                    var feed = XDocument.Load(url, LoadOptions.PreserveWhitespace);
                    var titleNode = feed.Descendants(atomns + "title").FirstOrDefault();
                    if ( titleNode != null )
                        title = titleNode.Value;

                    if(String.IsNullOrWhiteSpace(title)) {
                        ModelState.AddModelError("Url", T("The feed has no title.").Text);
                    }
                }
                catch {
                    ModelState.AddModelError("Url", T("The url of the feed or its content is not valid.").Text);
                }

                if ( !ModelState.IsValid )
                    return View(new PackagingAddSourceViewModel { Url = url });

                _packagingSourceManager.AddSource(title, url);
                _notifier.Information(T("The feed has been added successfully."));

                return RedirectToAction("Sources");
            }
            catch ( Exception exception ) {
                _notifier.Error(T("Adding feed failed: {0}", exception.Message));
                return View(new PackagingAddSourceViewModel { Url = url });
            }
        }

        public ActionResult Modules(int? sourceId) {
            var selectedSource = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            var sources = selectedSource != null 
                ? new [] { selectedSource }
                : _packagingSourceManager.GetSources()
            ;

            return View("Modules", new PackagingExtensionsViewModel {
                Extensions = sources.SelectMany(source => _packagingSourceManager.GetModuleList(source)),
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                SelectedSource = selectedSource
            });
        }

        public ActionResult Themes(int? sourceId) {
            var selectedSource = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            var sources = selectedSource != null
                ? new[] { selectedSource }
                : _packagingSourceManager.GetSources()
            ;

            return View("Themes", new PackagingExtensionsViewModel {
                Extensions = sources.SelectMany(source => _packagingSourceManager.GetThemeList(source)),
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                SelectedSource = selectedSource
            });
        }

        public ActionResult Install(string packageId, string version, int sourceId, string redirectTo) {
            var source = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            if(source == null) {
                return HttpNotFound();
            }

            _packageManager.Install(packageId, version, source.FeedUrl, HostingEnvironment.MapPath("~/"));

            return RedirectToAction(redirectTo == "Themes" ? "Themes" : "Modules");
        }

        public ActionResult AddModule(string returnUrl) {
            return View();
        }

        [HttpPost, ActionName("AddModule")]
        public ActionResult AddModulePOST(string returnUrl) {
            // module not used for anything o2ther than display (and that only to not have object in the view 'T')
            try {
                if (string.IsNullOrWhiteSpace(Request.Files[0].FileName)) {
                    ModelState.AddModelError("File", T("Select a file to upload.").ToString());
                }

                foreach (string fileName in Request.Files) {
                    HttpPostedFileBase file = Request.Files[fileName];
                    if (file != null) {
                        string fullFileName = Path.Combine(_appDataFolderRoot.RootFolder, fileName + ".nupkg").Replace(Path.DirectorySeparatorChar, '/');
                        file.SaveAs(fullFileName);
                        PackageInfo info = _packageManager.Install(new ZipPackage(fullFileName), _appDataFolderRoot.RootFolder, HostingEnvironment.MapPath("~/"));
                        System.IO.File.Delete(fullFileName);

                        _notifier.Information(T("Installed package \"{0}\", version {1} of type \"{2}\" at location \"{3}\"",
                            info.ExtensionName, info.ExtensionVersion, info.ExtensionType, info.ExtensionPath));
                    }
                }

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Modules");
            } catch (Exception exception) {
                for (var scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(T("Uploading module package failed: {0}", exception.Message));
                }

                return View("AddModule");
            }
        }
    }
}