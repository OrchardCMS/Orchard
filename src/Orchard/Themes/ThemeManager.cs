using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes {
    public class ThemeManager : IThemeManager {
        private readonly IEnumerable<IThemeSelector> _themeSelectors;
        private readonly IExtensionManager _extensionManager;

        public ThemeManager(IEnumerable<IThemeSelector> themeSelectors,
                            IExtensionManager extensionManager) {
            _themeSelectors = themeSelectors;
            _extensionManager = extensionManager;
        }

        public ExtensionDescriptor GetRequestTheme(RequestContext requestContext) {
            var requestTheme = _themeSelectors
                .Select(x => x.GetTheme(requestContext))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority).ToList();

            if (!requestTheme.Any())
                return null;

            foreach (var theme in requestTheme) {
                var t = _extensionManager.GetExtension(theme.ThemeName);
                if (t != null)
                    return t;
            }

            return _extensionManager.GetExtension("SafeMode");
        }
    }
}