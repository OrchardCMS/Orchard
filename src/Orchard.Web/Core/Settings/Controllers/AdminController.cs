using System;
using System.Web.Mvc;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Notify;

namespace Orchard.Core.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller {
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public AdminController(ISiteService siteService, INotifier notifier) {
            _siteService = siteService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var model = new Orchard.Core.Settings.ViewModels.SettingsIndexViewModel { 
                SiteSettings = _siteService.GetSiteSettings().As<SiteModel>() };
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection input) {
            var viewModel = new SettingsIndexViewModel { SiteSettings = _siteService.GetSiteSettings().As<SiteModel>() };
            UpdateModel(viewModel.SiteSettings, input.ToValueProvider());
            try {
                return RedirectToAction("Index");
            }
            catch (Exception exception) {
                _notifier.Error("Editing Settings failed: " + exception.Message);
                return View();
            }
        }
    }
}
