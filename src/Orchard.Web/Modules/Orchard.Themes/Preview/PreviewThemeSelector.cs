using System.Web.Routing;
using Orchard.Localization;
namespace Orchard.Themes.Preview {
    public class PreviewThemeSelector : IThemeSelector {
        private readonly IPreviewTheme _previewTheme;

        public PreviewThemeSelector(IPreviewTheme previewTheme) {
            _previewTheme = previewTheme;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public ThemeSelectorResult GetTheme(RequestContext context) {
            var previewThemeName = _previewTheme.GetPreviewTheme();
            return string.IsNullOrEmpty(previewThemeName) ? null : new ThemeSelectorResult { Priority = 90, ThemeName = previewThemeName };
        }
        public string GetTheme()
        {
            return _previewTheme.GetPreviewTheme();
        }
        public bool CanSet { get { return false; } }
        public void SetTheme(string themeName)
        {
            _previewTheme.SetPreviewTheme(themeName);
        }

        public string Tag { get { return string.Empty; } }

        public LocalizedString DisplayName { get { return T("PreviewTheme"); } }

        public string Name { get { return "previewTheme"; } }
    }
}