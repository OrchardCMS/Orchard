using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Translations.Services;
using Orchard.UI.Admin;

namespace Orchard.Translations.Controllers {
    [Admin]
    [ValidateInput(false)]
    public class TranslationAdminController : Controller, IUpdateModel {
        private readonly ICultureManager _cultureManager;
        private readonly ITranslationsManager _translationManager;

        public TranslationAdminController(ICultureManager cultureManager,
            IShapeFactory shapeFactory,
            ITranslationsManager translationManager) {
            _cultureManager = cultureManager;
            _translationManager = translationManager;

            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var cultureNames = _cultureManager.ListCultures();

            var statistics = cultureNames
                .Select(cultureName => _translationManager.GetStatistic(cultureName))
                .ToList();

            var viewModel = Shape.ViewModel()
                .Statistics(statistics);

            return View(viewModel);
        }

        public ActionResult Edit(string cultureName) {
            var statistic = _translationManager.GetStatistic(cultureName);

            var translatableFeatures = _translationManager.GetTranslatableProjects(cultureName);

            var viewModel = Shape.ViewModel()
                .Statistic(statistic)
                .Translatable(translatableFeatures);

            return View(viewModel);
        }

        public ActionResult Reset(string cultureName) {
            _translationManager.Reset(cultureName);

            return RedirectToAction("Index");
        }

        public ActionResult Delete(string cultureName) {
            _translationManager.Delete(cultureName);

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}