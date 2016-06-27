using System;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Providers;
using Orchard.Themes.Services;

namespace Orchard.Layouts.Services {
    /// <summary>
    /// Enables the rendering of shape templates from the admin while the shape templates reside in the current theme.
    /// </summary>
    [OrchardFeature("Orchard.Layouts.Snippets")]
    public class CurrentThemeShapeBindingResolver : ICurrentThemeShapeBindingResolver, IShapeBindingResolver, IDisposable {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IShapeTableLocator _shapeTableLocator;

        public CurrentThemeShapeBindingResolver(ISiteThemeService siteThemeService, IShapeTableLocator shapeTableLocator) {
            _siteThemeService = siteThemeService;
            _shapeTableLocator = shapeTableLocator;
        }

        public bool Enabled { get; private set; }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding) {
            shapeBinding = null;

            if (!Enabled)
                return false;

            var currentThemeName = _siteThemeService.GetCurrentThemeName();
            var shapeTable = _shapeTableLocator.Lookup(currentThemeName);
            return shapeTable.Bindings.TryGetValue(shapeType, out shapeBinding);
        }

        public IDisposable Enable() {
            Enabled = true;
            return this;
        }

        public void Dispose() {
            Enabled = false;
        }
    }
}