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
        private readonly dynamic _shapeFactory;

        public PreviewThemeFilter(
            IThemeService themeService, 
            IPreviewTheme previewTheme, 
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _themeService = themeService;
            _previewTheme = previewTheme;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var previewThemeName = _previewTheme.GetPreviewTheme();
            if (string.IsNullOrEmpty(previewThemeName))
                return;

            var installedThemes = _themeService.GetInstalledThemes();
            var themeListItems = installedThemes
                .Select(theme => new SelectListItem {
                    Text = theme.DisplayName,
                    Value = theme.Name,
                    Selected = theme.Name == previewThemeName
                })
                .ToList();


            
            _workContextAccessor.GetContext(filterContext).Layout.Zones["Body"].Add(_shapeFactory.ThemePreview(Themes: themeListItems), ":before");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) { }
    }
}