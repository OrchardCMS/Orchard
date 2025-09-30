using System;
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

        public UserCultureSelectorController(
            IOrchardServices services,
            ILocalizationService localizationService,
            ICultureStorageProvider cultureStorageProvider) {
            Services = services;
            _localizationService = localizationService;
            _cultureStorageProvider = cultureStorageProvider;
        }

        public ActionResult ChangeCulture(string culture) {
            if (string.IsNullOrEmpty(culture)) {
                throw new ArgumentNullException(culture);
            }

            var returnUrl = Utils.GetReturnUrl(Services.WorkContext.HttpContext.Request);
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "";

            if (_localizationService.TryGetRouteForUrl(returnUrl, out AutoroutePart currentRoutePart)
                && _localizationService.TryFindLocalizedRoute(currentRoutePart.ContentItem, culture, out AutoroutePart localizedRoutePart)) {
                returnUrl = localizedRoutePart.Path;
            }

            _cultureStorageProvider.SetCulture(culture);
            if (!returnUrl.StartsWith("~/")) {
                returnUrl = "~/" + returnUrl;
            }

            return this.RedirectLocal(returnUrl);
        }
    }
}
