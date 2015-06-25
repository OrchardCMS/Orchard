using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.OutputCache.Models;
using Orchard.Themes;

namespace Orchard.OutputCache.Providers {
    public class ThemeOutputCacheKeyCompositeProvider : IOutputCacheKeyCompositeProvider
    {
        private readonly IThemeManager _themeManager;

        public ThemeOutputCacheKeyCompositeProvider(IThemeManager themeManager) {
            _themeManager = themeManager;
        }

        public IEnumerable<KeyValuePair<string, object>> GetCacheKeySegment(ActionExecutingContext context, CacheSettings settings) {
            // Vary by theme.
            yield return new KeyValuePair<string, object>("theme", _themeManager.GetRequestTheme(context.RequestContext).Id.ToLowerInvariant());
        }
    }
}