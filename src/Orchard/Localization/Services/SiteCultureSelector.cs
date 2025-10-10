using System;
using System.Web;
using Orchard.Caching;

namespace Orchard.Localization.Services {
    public class SiteCultureSelector : ICultureSelector {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public SiteCultureSelector(
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) {

            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            var cacheKey = "Orchard.Localization.Services.SiteCultureSelector.GetCulture";
            return _cacheManager.Get(cacheKey, true, ctx => {
                // this is the same signal used in Orchard.Framework.DefaultCultureManager
                ctx.Monitor(_signals.When("culturesChanged"));

                string currentCultureName = _workContextAccessor.GetContext().CurrentSite.SiteCulture;
                if (String.IsNullOrEmpty(currentCultureName)) {
                    return null;
                }
                return new CultureSelectorResult { Priority = -5, CultureName = currentCultureName };
            });
            
        }
    }
}
