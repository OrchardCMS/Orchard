using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.Logging;
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
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResultBase = filterContext.Result as ViewResultBase;
            if (viewResultBase == null) {
                return;
            }

            //TODO: factor out into a service apart from the filter
            //TODO: add layout engine first

            var requestTheme = _themeService.GetRequestTheme(filterContext.RequestContext);
            var themeViewEngines = Enumerable.Empty<IViewEngine>();

            // todo: refactor. also this will probably result in the "SafeMode" theme being used so dump some debug info
            //   into the context for the theme to use for displaying why the expected theme isn't being used
            if (requestTheme != null) {
                var themeLocation = _extensionManager.GetThemeLocation(requestTheme);

                themeViewEngines = _viewEngineProviders
                    .Select(x => x.CreateThemeViewEngine(new CreateThemeViewEngineParams { VirtualPath = themeLocation }));
                Logger.Debug("Theme location:\r\n\t-{0}", themeLocation);
            }


            var modules = _extensionManager.AvailableExtensions()
                .Where(x => x.ExtensionType == "Module");

            var moduleLocations = modules.Select(x => Path.Combine(x.Location, x.Name));
            var moduleViewEngines = _viewEngineProviders
                .Select(x => x.CreateModulesViewEngine(new CreateModulesViewEngineParams { VirtualPaths = moduleLocations }));
            Logger.Debug("Module locations:\r\n\t-{0}", string.Join("\r\n\t-", moduleLocations.ToArray()));

            var requestViewEngines = new ViewEngineCollection(
                themeViewEngines
                    .Concat(moduleViewEngines)
                    .Concat(_viewEngines.Where(x => x.GetType().Assembly != typeof(LayoutViewEngine).Assembly))
                    .ToArray());

            var layoutViewEngine = new LayoutViewEngine(requestViewEngines);

            viewResultBase.ViewEngineCollection = new ViewEngineCollection(_viewEngines.ToList());
            viewResultBase.ViewEngineCollection.Insert(0, layoutViewEngine);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}