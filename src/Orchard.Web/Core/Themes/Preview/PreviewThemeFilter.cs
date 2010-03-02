using System.Linq;
using System.Web.Mvc;
using Orchard.Core.Themes.ViewModels;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.Themes;

namespace Orchard.Core.Themes.Preview {
    public class PreviewThemeFilter : FilterProvider, IResultFilter {
        private readonly IThemeService _themeService;
        private readonly IPreviewTheme _previewTheme;

        public PreviewThemeFilter(IThemeService themeService, IPreviewTheme previewTheme) {
            _themeService = themeService;
            _previewTheme = previewTheme;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var baseViewModel = BaseViewModel.From(filterContext.Result);
            if (baseViewModel == null)
                return;

            var previewThemeName = _previewTheme.GetPreviewTheme();
            if (string.IsNullOrEmpty(previewThemeName))
                return;

            var themes = _themeService.GetInstalledThemes();
            var model = new PreviewViewModel {
                Themes = themes.Select(theme => new SelectListItem {
                    Text = theme.DisplayName,
                    Value = theme.ThemeName,
                    Selected = theme.ThemeName == previewThemeName
                })
            };
            baseViewModel.Zones.AddRenderPartial("body:before", "Admin/ThemePreview", model);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}
