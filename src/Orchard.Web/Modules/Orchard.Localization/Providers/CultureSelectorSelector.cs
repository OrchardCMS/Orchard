using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;

namespace Orchard.Localization.Providers {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CultureSelectorSelector : ICultureSelector {
        private readonly ICultureService _cultureService;

        public CultureSelectorSelector(ICultureService cultureService) {
            _cultureService = cultureService;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            var cultureName = _cultureService.GetCulture();
            if (cultureName == null) return null;

            return new CultureSelectorResult { Priority = -4, CultureName = cultureName };
        }
    }
}