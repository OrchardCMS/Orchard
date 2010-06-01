using System;
using System.Web;
using JetBrains.Annotations;
using Orchard.Settings;

namespace Orchard.Localization {
    public class SiteCultureSelector : ICultureSelector {
       protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public CultureSelectorResult GetCulture(HttpContext context) {
            string currentCultureName = CurrentSite.SiteCulture;

            if (String.IsNullOrEmpty(currentCultureName)) {
                return null;
            }

            return new CultureSelectorResult { Priority = -5, CultureName = currentCultureName };
        }
    }

    public class CultureSettings {
    }
}
