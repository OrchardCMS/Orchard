using System.Web.Routing;

namespace Orchard.Themes.Preview {
    public class PreviewThemeSelector : IThemeSelector {
        private readonly IPreviewTheme _previewTheme;

        public PreviewThemeSelector(IPreviewTheme previewTheme) {
            _previewTheme = previewTheme;
        }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            var previewThemeName = _previewTheme.GetPreviewTheme();
            return string.IsNullOrEmpty(previewThemeName) ? null : new ThemeSelectorResult { Priority = 90, ThemeName = previewThemeName };
        }
    }
}