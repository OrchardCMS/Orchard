using System;
using Orchard.Mvc;

namespace Orchard.Themes.Preview {
    public class PreviewTheme : IPreviewTheme {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly string PreviewThemeKey = typeof(PreviewTheme).FullName;

        public PreviewTheme(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetPreviewTheme() {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext.Session != null) {
                return Convert.ToString(httpContext.Session[PreviewThemeKey]);
            }

            return null;
        }

        public void SetPreviewTheme(string themeName) {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext.Session != null) {
                if (string.IsNullOrEmpty(themeName)) {
                    httpContext.Session.Remove(PreviewThemeKey);
                }
                else {
                    httpContext.Session[PreviewThemeKey] = themeName;
                }
            }
        }
    }
}