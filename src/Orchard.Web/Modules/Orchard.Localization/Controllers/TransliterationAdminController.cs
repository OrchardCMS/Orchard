using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Localization.ViewModels;
using Orchard.UI.Admin;

namespace Orchard.Localization.Controllers {
    [OrchardFeature("Orchard.Localization.Transliteration")]
    [Admin]
    public class TransliterationAdminController : Controller {
        private readonly ITransliterationService _transliterationService;

        public TransliterationAdminController(ITransliterationService transliterationService,
            IShapeFactory shapeFactory) {
            _transliterationService = transliterationService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {

            var specifications = _transliterationService.GetSpecifications();

            var viewModel = Shape.ViewModel()
                .Specifications(specifications);

            return View(viewModel);
        }

        public ActionResult Create() {
            var viewModel = new CreateTransliterationViewModel();

            return View(viewModel);
        }
    }
}