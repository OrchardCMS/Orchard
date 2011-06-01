using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using ClaySharp.Implementation;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Zones;

namespace Orchard.ContentManagement {
    public class DefaultContentDisplay : IContentDisplay {
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly Lazy<IThemeManager> _themeService;
        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public DefaultContentDisplay(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeFactory shapeFactory,
            IShapeTableManager shapeTableManager,
            Lazy<IThemeManager> themeService,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider) {

            _handlers = handlers;
            _shapeFactory = shapeFactory;
            _shapeTableManager = shapeTableManager;
            _themeService = themeService;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public dynamic BuildDisplay(IContent content, string displayType, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype;
            var actualDisplayType = string.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(itemShape, content, actualDisplayType, groupId, _shapeFactory);
            BindPlacement(context, actualDisplayType, stereotype);

            _handlers.Value.Invoke(handler => handler.BuildDisplay(context), Logger);
            return context.Shape;
        }

        public dynamic BuildEditor(IContent content, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var context = new BuildEditorContext(itemShape, content, groupId, _shapeFactory);
            BindPlacement(context, null, stereotype);

            _handlers.Value.Invoke(handler => handler.BuildEditor(context), Logger);

            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, IUpdateModel updater, string groupInfoId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var context = new UpdateEditorContext(itemShape, content, updater, groupInfoId, _shapeFactory);
            BindPlacement(context, null, stereotype);

            _handlers.Value.Invoke(handler => handler.UpdateEditor(context), Logger);
            
            return context.Shape;
        }

        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHoldingBehavior = new ZoneHoldingBehavior(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            return _shapeFactory.Create(actualShapeType, Arguments.Empty(), new[] { zoneHoldingBehavior });
        }

        private void BindPlacement(BuildShapeContext context, string displayType, string stereotype) {
            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

                var theme = _themeService.Value.GetRequestTheme(_requestContext);
                var shapeTable = _shapeTableManager.GetShapeTable(theme.Id);
                var request = _requestContext.HttpContext.Request;

                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        ContentType = context.ContentItem.ContentType,
                        Stereotype = stereotype,
                        DisplayType = displayType,
                        Differentiator = differentiator,
                        Path = VirtualPathUtility.AppendTrailingSlash(_virtualPathProvider.ToAppRelative(request.Path)) // get the current app-relative path, i.e. ~/my-blog/foo
                    };

                    var placement = descriptor.Placement(placementContext);
                    if (placement != null) {
                        placement.Source = placementContext.Source;
                        return placement;
                    }
                }

                return new PlacementInfo {
                    Location = defaultLocation,
                    Source = String.Empty
                };
            };
        }
    }
}
