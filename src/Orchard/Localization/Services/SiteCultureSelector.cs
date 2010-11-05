using System;
using System.Web;

namespace Orchard.Localization.Services {
    public class SiteCultureSelector : ICultureSelector {
        private readonly IOrchardServices _orchardServices;

        public SiteCultureSelector(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            string currentCultureName = _orchardServices.WorkContext.CurrentSite.SiteCulture;

            if (String.IsNullOrEmpty(currentCultureName)) {
                return null;
            }

            return new CultureSelectorResult { Priority = -5, CultureName = currentCultureName };
        }
    }

    public class CultureSettings {
    }
}
