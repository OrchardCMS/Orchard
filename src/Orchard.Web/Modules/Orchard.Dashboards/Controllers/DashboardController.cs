using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Settings;
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
            if (!_services.Authorizer.Authorize(Permissions.ManageDashboards))
                return new HttpUnauthorizedResult();

            var dashboard = _dashboardService.GetDashboardDescriptor();
            var editor = dashboard.Editor(_services.New);
            return View(editor);
        }

        [ActionName("Edit")]
        [HttpPost]
        [FormValueRequired("submit.Save")]
        public ActionResult Save() {
            if (!_services.Authorizer.Authorize(Permissions.ManageDashboards))
                return new HttpUnauthorizedResult();

            return UpdateDashboard(dashboard => {
                if (!dashboard.Has<IPublishingControlAspect>() && !dashboard.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _services.ContentManager.Publish(dashboard);
                _services.Notifier.Success(T("Your dashboard has been saved."));
            });
        }

        [ActionName("Edit")]
        [HttpPost]
        [FormValueRequired("submit.Publish")]
        public ActionResult Publish() {
            if (!_services.Authorizer.Authorize(Permissions.ManageDashboards))
                return new HttpUnauthorizedResult();

            return UpdateDashboard(dashboard => {
                _services.ContentManager.Publish(dashboard);
                _services.Notifier.Success(T("Your dashboard has been published."));
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
            else
                _services.Notifier.Success(T("Your dashboard has been saved."));

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