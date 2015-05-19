using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Dashboards.Services;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Dashboards.Controllers {
    [Admin]
    [ValidateInput(false)]
    public class DashboardController : Controller, IUpdateModel {
        private readonly IDashboardService _dashboardService;
        private readonly IOrchardServices _services;

        public DashboardController(IDashboardService dashboardService, IOrchardServices services) {
            _dashboardService = dashboardService;
            _services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Display() {
            var shape = _dashboardService.GetDashboardShape();
            return new ShapeResult(this, shape);
        }

        public ActionResult Edit() {
            var dashboard = _dashboardService.GetDashboardDescriptor();
            var editor = dashboard.Editor(_services.New);
            return View(editor);
        }

        [ActionName("Edit")]
        [HttpPost]
        [FormValueRequired("submit.Save")]
        public ActionResult Save() {
            return UpdateDashboard(dashboard => _services.Notifier.Information(T("Your dashboard has been saved.")));
        }

        [ActionName("Edit")]
        [HttpPost]
        [FormValueRequired("submit.Publish")]
        public ActionResult Publish() {
            return UpdateDashboard(dashboard => {
                _services.Notifier.Information(T("Your dashboard has been published."));
                _services.ContentManager.Publish(dashboard);
            });
        }

        private ActionResult UpdateDashboard(Action<ContentItem> conditonallyPublish) {
            var dashboard = _dashboardService.GetDashboardDescriptor();
            var editor = dashboard.UpdateEditor(_services.New, this);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return View(editor);
            }

            var contentItem = (ContentItem)editor.ContentItem;

            if (contentItem != null)
                conditonallyPublish(contentItem);

            return RedirectToAction("Edit");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}