using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Extensions;
using Orchard.Mvc.Filters;
using Orchard.Themes;

namespace Orchard.Mvc.ViewEngines {

    public class ViewEngineFilter : FilterProvider, IResultFilter {
        private readonly ViewEngineCollection _viewEngines;
        private readonly IThemeService _themeService;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IViewEngineProvider> _viewEngineProviders;

        public ViewEngineFilter(
            ViewEngineCollection viewEngines,
            IThemeService themeService,
            IExtensionManager extensionManager,
            IEnumerable<IViewEngineProvider> viewEngineProviders) {
            _viewEngines = viewEngines;
            _themeService = themeService;
            _extensionManager = extensionManager;
            _viewEngineProviders = viewEngineProviders;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResultBase = filterContext.Result as ViewResultBase;
            if (viewResultBase == null) {
                return;
            }

            //TODO: factor out into a service apart from the filter
            //TODO: add layout engine first

            var requestTheme = _themeService.GetRequestTheme(filterContext.RequestContext);

            var theme = _extensionManager
                .AvailableExtensions()
                .Single(x => x.ExtensionType == "Theme" && x.Name == requestTheme.ThemeName);

            var themeLocation = Path.Combine(theme.Location, theme.Name);

            var themeViewEngines = _viewEngineProviders
                .Select(x => x.CreateThemeViewEngine(new CreateThemeViewEngineParams { VirtualPath = themeLocation }));

            var packages = _extensionManager.ActiveExtensions()
                .Where(x => x.Descriptor.ExtensionType == "Package");

            var packageLocations = packages.Select(x => Path.Combine(x.Descriptor.Location, x.Descriptor.Name));
            var packageViewEngines = _viewEngineProviders
                .Select(x => x.CreatePackagesViewEngine(new CreatePackagesViewEngineParams { VirtualPaths = packageLocations }));

            var requestViewEngines = new ViewEngineCollection(
                themeViewEngines
                    .Concat(packageViewEngines)
                    .Concat(_viewEngines)
                    .ToArray());

            var layoutViewEngine = new LayoutViewEngine(requestViewEngines);

            viewResultBase.ViewEngineCollection = new ViewEngineCollection(_viewEngines);
            viewResultBase.ViewEngineCollection.Insert(0, layoutViewEngine);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}