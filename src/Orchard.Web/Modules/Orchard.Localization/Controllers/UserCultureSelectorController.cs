using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Autoroute.Models;
using Orchard.CulturePicker.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization.Providers;
using Orchard.Localization.Services;
using Orchard.Mvc.Extensions;

namespace Orchard.Localization.Controllers {
    [OrchardFeature("Orchard.Localization.CultureSelector")]
    public class UserCultureSelectorController : Controller {
        private readonly ILocalizationService _localizationService;
        private readonly ICultureStorageProvider _cultureStorageProvider;
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public UserCultureSelectorController(IOrchardServices services, ILocalizationService localizationService, ICultureStorageProvider cultureStorageProvider) {
            Services = services;
            _localizationService = localizationService;
            _cultureStorageProvider = cultureStorageProvider;
        }

        public ActionResult ChangeCulture(string culture) {//, string returnUrl) {
            if (string.IsNullOrEmpty(culture)) {
                throw new ArgumentNullException(culture);
            }

            var returnUrl = Utils.GetReturnUrl(Services.WorkContext.HttpContext.Request);
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "";

            AutoroutePart currentRoutePart;
            if (_localizationService.TryGetRouteForUrl(returnUrl, out currentRoutePart)) {
                AutoroutePart localizedRoutePart;
                if (_localizationService.TryFindLocalizedRoute(currentRoutePart.ContentItem, culture, out localizedRoutePart)) {
                    returnUrl = localizedRoutePart.Path;
                }
            }

            _cultureStorageProvider.SetCulture(culture);
            if (!returnUrl.StartsWith("~//")) {
                returnUrl = "~//" + returnUrl;
            }
            ActionResult r = this.RedirectLocal(returnUrl + "?culture=" + culture);
            return r;
        }
    }
}