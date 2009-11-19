using System.Web.Mvc;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.ViewModels;
using Orchard.Localization;
using Orchard.Models;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.Models.Driver;

namespace Orchard.Core.Settings.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly IContentManager _modelManager;
        private readonly INotifier _notifier;

        public AdminController(ISiteService siteService, IContentManager modelManager, INotifier notifier) {
            _siteService = siteService;
            _modelManager = modelManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var model = new Orchard.Core.Settings.ViewModels.SettingsIndexViewModel { 
                Site = _siteService.GetSiteSettings().As<SiteSettings>() };
            model.Editors = _modelManager.GetEditors(model.Site.ContentItem);
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection input) {
            var viewModel = new SettingsIndexViewModel { Site = _siteService.GetSiteSettings().As<SiteSettings>() };
            viewModel.Editors = _modelManager.UpdateEditors(viewModel.Site.ContentItem, this);

            if (!TryUpdateModel(viewModel, input.ToValueProvider())) {
                return View(viewModel);
            }

            _notifier.Information(T("Settings updated"));
            return RedirectToAction("Index");
        }

        #region IUpdateModel Members

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        #endregion
    }
}
