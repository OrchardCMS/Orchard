using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.ContentManagement {
    public class DefaultContentDisplay : IContentDisplay {
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeHelperFactory _shapeHelperFactory;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<IThemeService> _themeService;
        private readonly RequestContext _requestContext;

        public DefaultContentDisplay(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeHelperFactory shapeHelperFactory,
            IShapeTableManager shapeTableManager,
            IWorkContextAccessor workContextAccessor,
            IHttpContextAccessor httpContextAccessor,
            Lazy<IThemeService> themeService,
            RequestContext requestContext) {
            _handlers = handlers;
            _shapeHelperFactory = shapeHelperFactory;
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

            var shapeTypeName = "Items_" + stereotype;
            var shapeDisplayType = string.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;

            var shapeHelper = _shapeHelperFactory.CreateHelper();
            var itemShape = _shapeHelperCalls.Invoke(shapeHelper, shapeTypeName);

            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = shapeDisplayType;

            var context = new BuildDisplayContext(itemShape, content, shapeDisplayType, _shapeHelperFactory);
            BindPlacement(context, displayType);

            _handlers.Value.Invoke(handler => handler.BuildDisplay(context), Logger);
            return context.Shape;
        }

        public dynamic BuildEditor(IContent content) {
            var shapeHelper = _shapeHelperFactory.CreateHelper();
            var itemShape = shapeHelper.Items_Content_Edit();

            IContent iContent = content;
            if (iContent != null)
                itemShape.ContentItem = iContent.ContentItem;

            var context = new BuildEditorContext(itemShape, content, _shapeHelperFactory);
            BindPlacement(context, null);

            _handlers.Value.Invoke(handler => handler.BuildEditor(context), Logger);
            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, IUpdateModel updater) {
            var shapeHelper = _shapeHelperFactory.CreateHelper();
            var itemShape = shapeHelper.Items_Content_Edit();

            IContent iContent = content;
            if (iContent != null)
                itemShape.ContentItem = iContent.ContentItem;

            var context = new UpdateEditorContext(itemShape, content, updater, _shapeHelperFactory);
            BindPlacement(context, null);

            _handlers.Value.Invoke(handler => handler.UpdateEditor(context), Logger);
            return context.Shape;
        }

        private void BindPlacement(BuildShapeContext context, string displayType) {
            context.FindPlacement = (partShapeType, defaultLocation) => {
                //var workContext = _workContextAccessor.GetContext();
                //var theme = workContext.CurrentTheme;
                var theme = _themeService.Value.GetRequestTheme(_requestContext);
                var shapeTable = _shapeTableManager.GetShapeTable(theme.ThemeName);
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
