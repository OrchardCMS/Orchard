using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;
using Orchard.Themes.ViewModels;

namespace Orchard.Themes.Preview {
    public class PreviewThemeFilter : FilterProvider, IResultFilter {
        private readonly IThemeService _themeService;
        private readonly IPreviewTheme _previewTheme;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IShapeHelperFactory _shapeHelperFactory;

        public PreviewThemeFilter(IThemeService themeService, IPreviewTheme previewTheme, IWorkContextAccessor workContextAccessor, IShapeHelperFactory shapeHelperFactory) {
            _themeService = themeService;
            _previewTheme = previewTheme;
            _workContextAccessor = workContextAccessor;
            _shapeHelperFactory = shapeHelperFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
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

            var shape = _shapeHelperFactory.CreateHelper();
            _workContextAccessor.GetContext(filterContext).CurrentPage.Zones["Body"].Add(shape.ThemePreview(model), ":before");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}