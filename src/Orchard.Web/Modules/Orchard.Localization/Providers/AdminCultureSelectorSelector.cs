using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.UI.Admin;

namespace Orchard.Localization.Providers {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class AdminCultureSelectorSelector : ICultureSelector {
        private readonly IEnumerable<ICultureProvider> _providers;

        public AdminCultureSelectorSelector(IEnumerable<ICultureProvider> providers) {
            _providers = providers;
        }

        private bool IsActivable(HttpContextBase context) {
            // activate on admin screen only
            if (AdminFilter.IsApplied(new RequestContext(context, new RouteData())))
                return true;

            return false;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (!IsActivable(context))
                return null;

            var cultureName = _providers
                .OrderByDescending(x => x.Priority)
                .Select(x => x.GetCulture())
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(cultureName)) return null;

            return new CultureSelectorResult { Priority = -4, CultureName = cultureName };
        }
    }
}