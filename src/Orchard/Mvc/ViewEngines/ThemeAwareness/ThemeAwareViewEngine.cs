using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Themes;

namespace Orchard.Mvc.ViewEngines.ThemeAwareness {
    public interface IThemeAwareViewEngine : IDependency, IViewEngine {
    }

    public class ThemeAwareViewEngine : IThemeAwareViewEngine {
        private readonly WorkContext _workContext;
        private readonly IEnumerable<IViewEngineProvider> _viewEngineProviders;
        private readonly IConfiguredEnginesCache _configuredEnginesCache;
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IViewEngine _nullEngines = new ViewEngineCollectionWrapper(Enumerable.Empty<IViewEngine>());

        public ThemeAwareViewEngine(
            WorkContext workContext,
            IEnumerable<IViewEngineProvider> viewEngineProviders,
            IConfiguredEnginesCache configuredEnginesCache,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor) {
            _workContext = workContext;
            _viewEngineProviders = viewEngineProviders;
            _configuredEnginesCache = configuredEnginesCache;
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {
            IViewEngine engines = _nullEngines;

            if (partialViewName.StartsWith("/") || partialViewName.StartsWith("~")) {
                engines = BareEngines();
            }
            else if (_workContext.CurrentTheme != null) {
                if (controllerContext is ViewContext) {
                    engines = ShallowEngines(_workContext.CurrentTheme);
                }
                else {
                    engines = DeepEngines(_workContext.CurrentTheme);
                }
            }

            return engines.FindPartialView(controllerContext, partialViewName, useCache);
        }

        private IViewEngine BareEngines() {
            return _configuredEnginesCache.BindBareEngines(() => new ViewEngineCollectionWrapper(_viewEngineProviders.Select(vep => vep.CreateBareViewEngine())));
        }

        private IViewEngine ShallowEngines(ITheme theme) {
            //return _configuredEnginesCache.BindShallowEngines(theme.ThemeName, () => new ViewEngineCollectionWrapper(_viewEngineProviders.Select(vep => vep.CreateBareViewEngine())));
            return DeepEngines(theme);
        }

        private IViewEngine DeepEngines(ITheme theme) {
            return _configuredEnginesCache.BindDeepEngines(theme.ThemeName, () => {
                var engines = Enumerable.Empty<IViewEngine>();
                var themeLocation = _extensionManager.GetThemeLocation(theme);

                var themeParams = new CreateThemeViewEngineParams { VirtualPath = themeLocation };
                engines = engines.Concat(_viewEngineProviders.Select(vep => vep.CreateThemeViewEngine(themeParams)));

                var activeFeatures = _extensionManager.AvailableFeatures().Where(fd => _shellDescriptor.Features.Any(sdf => sdf.Name == fd.Name));
                var activeModuleLocations = activeFeatures.Select(fd => fd.Extension.Location + "/" + fd.Extension.Name).Distinct();
                var moduleParams = new CreateModulesViewEngineParams { VirtualPaths = activeModuleLocations };
                engines = engines.Concat(_viewEngineProviders.Select(vep => vep.CreateModulesViewEngine(moduleParams)));

                return new ViewEngineCollectionWrapper(engines);
            });
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {
            return FindPartialView(controllerContext, viewName, useCache);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view) {
            throw new NotImplementedException();
        }
    }

}
