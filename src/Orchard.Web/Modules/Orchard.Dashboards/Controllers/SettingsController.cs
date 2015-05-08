using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Dashboards.Models;
using Orchard.Dashboards.ViewModels;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Dashboards.Controllers {
    [Admin]
    public class SettingsController : Controller {
        private readonly IOrchardServices _services;
        
        public SettingsController(IOrchardServices services) {
            _services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var settings = _services.WorkContext.CurrentSite.As<DashboardSiteSettingsPart>();
            var viewModel = new DashboardSiteSettingsViewModel {
                SelectedDashboardId = settings.DefaultDashboardId.ToString(),
                SelectedDashboard = settings.DefaultDashboardId != null ? _services.ContentManager.Get(settings.DefaultDashboardId.Value) : default(ContentItem)
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(DashboardSiteSettingsViewModel viewModel) {
            var settings = _services.WorkContext.CurrentSite.As<DashboardSiteSettingsPart>();

            if (!ModelState.IsValid) {
                viewModel.SelectedDashboard = settings.DefaultDashboardId != null ? _services.ContentManager.Get(settings.DefaultDashboardId.Value) : default(ContentItem);
                return View(viewModel);
            }
                
            settings.DefaultDashboardId = ParseContentId(viewModel.SelectedDashboardId);
            _services.Notifier.Information(T("Dashboard settings updated."));

            return RedirectToAction("Index");
        }

        private static int? ParseContentId(string value) {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            var items = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!items.Any())
                return null;

            return XmlHelper.Parse<int>(items.First());
        }
    }
}