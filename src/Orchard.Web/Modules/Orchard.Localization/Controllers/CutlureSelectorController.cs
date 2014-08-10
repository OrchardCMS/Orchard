using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Mvc.Extensions;

namespace Orchard.Localization.Controllers {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CutlureSelectorController : Controller {
        private readonly ICultureService _cultureService;

        public CutlureSelectorController(ICultureService cultureService) {
            _cultureService = cultureService;
        }

        public ActionResult SetCulture(string culture, string returnUrl) {

            _cultureService.SetCulture(culture);

            return this.RedirectLocal(returnUrl);
        }
    }
}