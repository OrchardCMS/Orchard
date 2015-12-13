using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Mvc.Filters;

namespace Orchard.Themes.Preview {
    public class PreviewThemeFilter : FilterProvider, IResultFilter {
        private readonly IPreviewTheme _previewTheme;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;
        private readonly IFeatureManager _featureManager;

        public PreviewThemeFilter(
            IPreviewTheme previewTheme,
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory,
            IFeatureManager featureManager) {
            _previewTheme = previewTheme;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
            _featureManager = featureManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            if(filterContext.Result as ViewResult == null) {
                return;
            }

            var previewThemeName = _previewTheme.GetPreviewTheme();
            if (string.IsNullOrEmpty(previewThemeName))
                return;

            var installedThemes = _featureManager.GetEnabledFeatures()
                .Select(x => x.Extension)
                .Where(x =>  DefaultExtensionTypes.IsTheme(x.ExtensionType))
                .Distinct();

            var themeListItems = installedThemes
                .Select(theme => new SelectListItem {
                    Text = theme.Name,
                    Value = theme.Id,
                    Selected = theme.Id == previewThemeName
                })
                .ToList();

            _workContextAccessor.GetContext(filterContext).Layout.Zones["Body"].Add(_shapeFactory.ThemePreview(Themes: themeListItems), ":before");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) { }
    }
}