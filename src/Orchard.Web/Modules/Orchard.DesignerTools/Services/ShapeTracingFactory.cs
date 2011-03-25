using System.IO;
using System.Linq;
using System.Web.Routing;
using System.Xml;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI;
using Orchard.UI.Admin;

namespace Orchard.DesignerTools.Services {
    [OrchardFeature("Orchard.DesignerTools")]
    public class ShapeTracingFactory : IShapeFactoryEvents, IShapeDisplayEvents {
        private readonly WorkContext _workContext;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IThemeManager _themeManager;
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IAuthorizer _authorizer;
        private int shapeId = 0;

        public ShapeTracingFactory(
            WorkContext workContext, 
            IShapeTableManager shapeTableManager, 
            IThemeManager themeManager, 
            IWebSiteFolder webSiteFolder,
            IAuthorizer authorizer
            ) {
            _workContext = workContext;
            _shapeTableManager = shapeTableManager;
            _themeManager = themeManager;
            _webSiteFolder = webSiteFolder;
            _authorizer = authorizer;
        }

        private bool IsActivable() {
            // activate on front-end only
            if (AdminFilter.IsApplied(new RequestContext(_workContext.HttpContext, new RouteData())))
                return false;

            // if not logged as a site owner, still activate if it's a local request (development machine)
            if (!_authorizer.Authorize(StandardPermissions.SiteOwner))
                return _workContext.HttpContext.Request.IsLocal;

            return true;
        }

        public void Creating(ShapeCreatingContext context) {
        }

        public void Created(ShapeCreatedContext context) {
            if(!IsActivable()) {
                return;
            }

            if (context.ShapeType != "Layout"
                && context.ShapeType != "DocumentZone"
                && context.ShapeType != "PlaceChildContent"
                && context.ShapeType != "ContentZone"
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
            if (!IsActivable()) {
                return;
            }

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
                el.WriteTo(new XmlTextWriter(sw) {Formatting = Formatting.None});
                context.Shape._Dump = sw.ToString();
            }

            shape.Template = null;
            shape.OriginalTemplate = descriptor.BindingSource;

            foreach (var extension in new[] { ".cshtml", ".aspx" }) {
                foreach (var alternate in shapeMetadata.Alternates.Reverse()) {
                    var alternateFilename = currentTheme.Location + "/" + currentTheme.Id + "/Views/" + alternate.Replace("__", "-").Replace("_", ".") + extension;
                    if (_webSiteFolder.FileExists(alternateFilename)) {
                        shape.Template = alternateFilename;
                    }
                }
            }

            if(shape.Template == null) {
                shape.Template = descriptor.BindingSource;
            }

            if(shape.Template == null) {
                shape.Template = descriptor.Bindings.Values.FirstOrDefault().BindingSource;
            }

            if (shape.OriginalTemplate == null) {
                shape.OriginalTemplate = descriptor.Bindings.Values.FirstOrDefault().BindingSource;
            }

            try {
                if (_webSiteFolder.FileExists(shape.Template)) {
                    shape.TemplateContent = _webSiteFolder.ReadFile(shape.Template);
                }
            }
            catch {
                // the url might be invalid in case of a code shape
            }

            if (shapeMetadata.PlacementSource != null && _webSiteFolder.FileExists(shapeMetadata.PlacementSource)) {
                context.Shape.PlacementContent = _webSiteFolder.ReadFile(shapeMetadata.PlacementSource);
            }

            // Inject the Zone name
            if(shapeMetadata.Type == "Zone") {
                shape.Hint = ((Zone) shape).ZoneName;
            }

            shape.ShapeId = shapeId++;
        }


        public void Displayed(ShapeDisplayedContext context) {
        }
    }
}