using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using ClaySharp;
using ClaySharp.Implementation;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Zones;

namespace Orchard.ContentManagement {
    public class DefaultContentDisplay : IContentDisplay {
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<IThemeService> _themeService;
        private readonly RequestContext _requestContext;

        public DefaultContentDisplay(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeFactory shapeFactory,
            IShapeTableManager shapeTableManager,
            IWorkContextAccessor workContextAccessor,
            IHttpContextAccessor httpContextAccessor,
            Lazy<IThemeService> themeService,
            RequestContext requestContext) {
            _handlers = handlers;
            _shapeFactory = shapeFactory;
            _shapeTableManager = shapeTableManager;
            _workContextAccessor = workContextAccessor;
            _httpContextAccessor = httpContextAccessor;
            _themeService = themeService;
            _requestContext = requestContext;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        static readonly CallSiteCollection _shapeHelperCalls = new CallSiteCollection(shapeTypeName => Binder.InvokeMember(
            CSharpBinderFlags.None,
            shapeTypeName,
            Enumerable.Empty<Type>(),
            null,
            new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));


        public dynamic BuildDisplay(IContent content, string displayType) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype;
            var actualDisplayType = string.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(itemShape, content, actualDisplayType, _shapeFactory);
            BindPlacement(context, actualDisplayType);

            _handlers.Value.Invoke(handler => handler.BuildDisplay(context), Logger);
            return context.Shape;
        }

        public dynamic BuildEditor(IContent content) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var context = new BuildEditorContext(itemShape, content, _shapeFactory);
            BindPlacement(context, null);

            _handlers.Value.Invoke(handler => handler.BuildEditor(context), Logger);
            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, IUpdateModel updater) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var context = new UpdateEditorContext(itemShape, content, updater, _shapeFactory);
            BindPlacement(context, null);

            _handlers.Value.Invoke(handler => handler.UpdateEditor(context), Logger);
            return context.Shape;
        }

        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHoldingBehavior = new ZoneHoldingBehavior(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            return _shapeFactory.Create(actualShapeType, Arguments.Empty(), new[] { zoneHoldingBehavior });
        }

        private void BindPlacement(BuildShapeContext context, string displayType) {
            context.FindPlacement = (partShapeType, defaultLocation) => {
                //var workContext = _workContextAccessor.GetContext();
                //var theme = workContext.CurrentTheme;
                var theme = _themeService.Value.GetRequestTheme(_requestContext);
                var shapeTable = _shapeTableManager.GetShapeTable(theme.Name);
                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        ContentType = context.ContentItem.ContentType,
                        DisplayType = displayType
                    };
                    var location = descriptor.Placement(placementContext);
                    return location ?? defaultLocation;
                }
                return defaultLocation;
            };
        }
    }
}
