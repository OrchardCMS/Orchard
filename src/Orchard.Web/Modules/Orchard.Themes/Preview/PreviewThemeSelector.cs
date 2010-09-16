using System.Web.Routing;

namespace Orchard.Themes.Preview {
    public class PreviewThemeSelector : IThemeSelector {
        private readonly IPreviewTheme _previewTheme;

        public PreviewThemeSelector(IPreviewTheme previewTheme) {
            _previewTheme = previewTheme;
        }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            var previewThemeName = _previewTheme.GetPreviewTheme();
            if (string.IsNullOrEmpty(previewThemeName))
                return null;

            return new ThemeSelectorResult { Priority = 90, ThemeName = previewThemeName };
        }
    }
}