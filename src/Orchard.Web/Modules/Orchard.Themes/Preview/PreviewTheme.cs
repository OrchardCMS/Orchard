using System;
using System.Web;
using System.Web.Routing;

namespace Orchard.Themes.Preview {
    public class PreviewTheme : IPreviewTheme, IThemeSelector {
        private static readonly string PreviewThemeKey = typeof(PreviewTheme).FullName;
        private readonly HttpContextBase _httpContext;

        public PreviewTheme(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        public string GetPreviewTheme() {
            return Convert.ToString(_httpContext.Session[PreviewThemeKey]);
        }

        public void SetPreviewTheme(string themeName) {
            if (string.IsNullOrEmpty(themeName)) {
                _httpContext.Session.Remove(PreviewThemeKey);
            }
            else {
                _httpContext.Session[PreviewThemeKey] = themeName;
            }
        }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            var previewThemeName = GetPreviewTheme();
            if (string.IsNullOrEmpty(previewThemeName))
                return null;

            return new ThemeSelectorResult { Priority = 90, ThemeName = previewThemeName };
        }

    }
}