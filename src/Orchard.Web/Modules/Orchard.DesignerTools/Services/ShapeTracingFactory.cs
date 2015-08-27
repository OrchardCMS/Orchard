using System;
using System.Linq;
using System.Text;
using System.Web.Routing;
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
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IThemeManager _themeManager;
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IAuthorizer _authorizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private bool _processing;

        private int _shapeId;

        public ShapeTracingFactory(
            IWorkContextAccessor workContextAccessor, 
            IShapeTableManager shapeTableManager, 
            IThemeManager themeManager, 
            IWebSiteFolder webSiteFolder,
            IAuthorizer authorizer
            ) {
            _workContextAccessor = workContextAccessor;
            _shapeTableManager = shapeTableManager;
            _themeManager = themeManager;
            _webSiteFolder = webSiteFolder;
            _authorizer = authorizer;
        }

        private bool IsActivable() {
            var workContext = _workContextAccessor.GetContext();

            // activate on front-end only
            if (AdminFilter.IsApplied(new RequestContext(workContext.HttpContext, new RouteData())))
                return false;

            // if not logged as a site owner, still activate if it's a local request (development machine)
            if (!_authorizer.Authorize(StandardPermissions.SiteOwner)) {
                
                return workContext.HttpContext.Request.IsLocal;
            }

            return true;
        }

        public void Creating(ShapeCreatingContext context) {
        }

        public void Created(ShapeCreatedContext context) {
            if(!IsActivable()) {
                return;
            }

            // prevent reentrance as some methods could create new shapes, and trigger this event
            if(_processing) {
                return;
            }

            _processing = true;

            if (context.ShapeType != "Layout"
                && context.ShapeType != "DocumentZone"
                && context.ShapeType != "PlaceChildContent"
                && context.ShapeType != "ContentZone"
                && context.ShapeType != "ShapeTracingMeta"
                && context.ShapeType != "ShapeTracingTemplates"
                && context.ShapeType != "DateTimeRelative") {

                var shapeMetadata = (ShapeMetadata)context.Shape.Metadata;
                var workContext = _workContextAccessor.GetContext();
                var currentTheme = workContext.CurrentTheme;
                var shapeTable = _shapeTableManager.GetShapeTable(currentTheme.Id);

                if (!shapeTable.Descriptors.ContainsKey(shapeMetadata.Type)) {
                    _processing = false;
                    return;
                }

                shapeMetadata.Wrappers.Add("ShapeTracingWrapper");
                shapeMetadata.OnDisplaying(OnDisplaying);
            }

            _processing = false;
        }
        public void Displaying(ShapeDisplayingContext context) {}

        public void OnDisplaying(ShapeDisplayingContext context) {
            if (!IsActivable()) {
                return;
            }

            var shape = context.Shape;
            var shapeMetadata = (ShapeMetadata) context.Shape.Metadata;
            var workContext = _workContextAccessor.GetContext();
            var currentTheme = _themeManager.GetRequestTheme(workContext.HttpContext.Request.RequestContext);
            var shapeTable = _shapeTableManager.GetShapeTable(currentTheme.Id);

            if (!shapeMetadata.Wrappers.Contains("ShapeTracingWrapper")) {
                return;
            }

            var descriptor = shapeTable.Descriptors[shapeMetadata.Type];

            // dump the Shape's content
            var dump = new ObjectDumper(6).Dump(context.Shape, "Model");

            var sb = new StringBuilder();
            ObjectDumper.ConvertToJSon(dump, sb);
            shape._Dump = sb.ToString();

            shape.Template = null;
            shape.OriginalTemplate = descriptor.BindingSource;

            foreach (var extension in new[] { ".cshtml", ".aspx" }) {
                foreach (var alternate in shapeMetadata.Alternates.Reverse().Concat(new [] {shapeMetadata.Type}) ) {
                    var alternateFilename = FormatShapeFilename(alternate, shapeMetadata.Type, shapeMetadata.DisplayType, currentTheme.Location + "/" + currentTheme.Id, extension);
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
                // we know that templates are classes if they contain ':'
                if (!shape.Template.Contains(":") && _webSiteFolder.FileExists(shape.Template)) {
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

            shape.ShapeId = _shapeId++;
        }


        public void Displayed(ShapeDisplayedContext context) {
        }

        private static string FormatShapeFilename(string shape, string shapeType, string displayType, string themePrefix, string extension) {

            if (!String.IsNullOrWhiteSpace(displayType)) {
                if (shape.StartsWith(shapeType + "_" + displayType)) {
                    shape = shapeType + shape.Substring(shapeType.Length + displayType.Length + 1) + "_" + displayType;
                }
            }

            return themePrefix + "/Views/" + shape.Replace("__", "-").Replace("_", ".") + extension;
        }

    }
}