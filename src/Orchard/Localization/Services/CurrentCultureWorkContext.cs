using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Mvc;

namespace Orchard.Localization.Services {
    public class CurrentCultureWorkContext : IWorkContextStateProvider {
        private readonly IEnumerable<ICultureSelector> _cultureSelectors;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICultureManager _cultureManager;

        public CurrentCultureWorkContext(IEnumerable<ICultureSelector> cultureSelectors,
            IHttpContextAccessor httpContextAccessor,
            ICultureManager cultureManager) {
            _cultureSelectors = cultureSelectors;
            _httpContextAccessor = httpContextAccessor;
            _cultureManager = cultureManager;
        }

        public Func<WorkContext, T> Get<T>(string name) {
            if (name == "CurrentCulture") {
                var cultureName = GetCurrentCulture();
                return ctx => (T)(object)cultureName;
            }
            return null;
        }

        private string GetCurrentCulture() {
            var httpContext = _httpContextAccessor.Current();

            var supportedCultures = _cultureManager.ListCultures().ToList();

            var culture = _cultureSelectors
                .Select(c => c.GetCulture(httpContext))
                .Where(c => c != null)
                .OrderByDescending(c => c.Priority)
                .FirstOrDefault(c => !String.IsNullOrEmpty(c.CultureName) && supportedCultures.Any(s => s.Equals(c.CultureName, StringComparison.OrdinalIgnoreCase)));

            return culture == null ? String.Empty : culture.CultureName;
        }
    }
}
