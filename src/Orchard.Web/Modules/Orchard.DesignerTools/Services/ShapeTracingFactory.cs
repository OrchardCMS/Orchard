using System.IO;
using System.Xml;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;
using Orchard.Themes;

namespace Orchard.DesignerTools.Services {
    [OrchardFeature("Orchard.DesignerTools")]
    public class ShapeTracingFactory : IShapeFactoryEvents, IShapeDisplayEvents {
        private readonly WorkContext _workContext;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IThemeManager _themeManager;
        private readonly IWebSiteFolder _webSiteFolder;

        public ShapeTracingFactory(WorkContext workContext, IShapeTableManager shapeTableManager, IThemeManager themeManager, IWebSiteFolder webSiteFolder) {
            _workContext = workContext;
            _shapeTableManager = shapeTableManager;
            _themeManager = themeManager;
            _webSiteFolder = webSiteFolder;
        }

        public void Creating(ShapeCreatingContext context) {
        }

        public void Created(ShapeCreatedContext context) {
            if (context.ShapeType != "Layout"
                && context.ShapeType != "DocumentZone"
                && context.ShapeType != "PlaceChildContent"
                && context.ShapeType != "HeadScripts"
                && context.ShapeType != "FootScripts"
                && context.ShapeType != "HeadLinks"
                && context.ShapeType != "FootLinks"
                && context.ShapeType != "StylesheeLinks"
                && context.ShapeType != "Style"
                && context.ShapeType != "Feed"
                && context.ShapeType != "Resource"
                && context.ShapeType != "Meta"
                // && context.ShapeType != "ContentZone"
                && context.ShapeType != "ShapeTracingMeta") {

                var shapeMetadata = (ShapeMetadata)context.Shape.Metadata;
                var currentTheme = _themeManager.GetRequestTheme(_workContext.HttpContext.Request.RequestContext);
                var shapeTable = _shapeTableManager.GetShapeTable(currentTheme.Id);

                if (!shapeTable.Descriptors.ContainsKey(shapeMetadata.Type)) {
                    return;
                }

                shapeMetadata.Wrappers.Add("ShapeTracingWrapper");
            }
        }

        public void Displaying(ShapeDisplayingContext context) {
            var shape = context.Shape;
            var shapeMetadata = (ShapeMetadata) context.Shape.Metadata;
            var currentTheme = _themeManager.GetRequestTheme(_workContext.HttpContext.Request.RequestContext);
            var shapeTable = _shapeTableManager.GetShapeTable(currentTheme.Id);

            if (!shapeMetadata.Wrappers.Contains("ShapeTracingWrapper")) {
                return;
            }

            var descriptor = shapeTable.Descriptors[shapeMetadata.Type];

            // dump the Shape's content
            var dumper = new ObjectDumper(6);
            var el = dumper.Dump(context.Shape, "Model");
            using (var sw = new StringWriter()) {
                el.WriteTo(new XmlTextWriter(sw) {Formatting = Formatting.Indented});
                context.Shape._Dump = sw.ToString();
            }

            shape._Definition = descriptor.BindingSource;

            try {
                if (_webSiteFolder.FileExists(descriptor.BindingSource)) {
                    shape._DefinitionContent = _webSiteFolder.ReadFile(descriptor.BindingSource);
                }
            }
            catch {
                // the url might be invalid in case of a code shape
            }

            if (shapeMetadata.PlacementSource != null && _webSiteFolder.FileExists(shapeMetadata.PlacementSource)) {
                context.Shape.PlacementContent = _webSiteFolder.ReadFile(shapeMetadata.PlacementSource);
            }
        }


        public void Displayed(ShapeDisplayedContext context) {
        }
    }
}