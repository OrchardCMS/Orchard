using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.Localization.Providers;
using Orchard.Mvc.Extensions;
using Orchard.UI.Admin;

namespace Orchard.Localization.Controllers {
    [OrchardFeature("Orchard.Localization.CultureSelector")]
    [Admin]
    public class AdminCultureSelectorController : Controller {
        private readonly ICultureStorageProvider _cultureStorageProvider;

        public AdminCultureSelectorController(ICultureStorageProvider cultureStorageProvider) {
            _cultureStorageProvider = cultureStorageProvider;
        }

        public ActionResult ChangeCulture(string culture, string returnUrl) {
            _cultureStorageProvider.SetCulture(culture);

            return this.RedirectLocal(returnUrl);
        }
    }
}