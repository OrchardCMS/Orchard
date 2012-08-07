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
using Orchard.UI.Zones;
using Orchard.Environment.Extensions;
using Four2n.Orchard.MiniProfiler.Services;

namespace Orchard.ContentManagement {
    [OrchardSuppressDependency("Orchard.ContentManagement.DefaultContentDisplay")]
    public class ProfilingContentDisplay : IContentDisplay {
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly IShapeFactory _shapeFactory;
        private readonly Lazy<IShapeTableLocator> _shapeTableLocator; 

        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IProfilerService _profiler;
        public ProfilingContentDisplay(
            Lazy<IEnumerable<IContentHandler>> handlers,
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator, 
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor,
            IProfilerService profiler) {

            _handlers = handlers;
            _shapeFactory = shapeFactory;
            _shapeTableLocator = shapeTableLocator;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;
            _workContextAccessor = workContextAccessor;
            _profiler = profiler;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public dynamic BuildDisplay(IContent content, string displayType, string groupId) {
            var contentKey = "ContentDisplay:" + content.Id.ToString();
            _profiler.StepStart(contentKey, String.Format("Content Display: {0} ({1})", content.Id, displayType));
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

            _handlers.Value.Invoke(handler => {
                var key = String.Format("ContentDisplay:{0}:{1}", content.Id, handler.GetType().FullName);
                _profiler.StepStart(key, String.Format("Content Display: {0}", handler.GetType().FullName));
                handler.BuildDisplay(context);
                _profiler.StepStop(key);
            }, Logger);
            _profiler.StepStop(contentKey);
            return context.Shape;
        }

        public dynamic BuildEditor(IContent content, string groupId) {
            var contentKey = "ContentEditor:" + content.Id.ToString();
            _profiler.StepStart(contentKey, String.Format("Content Editor: {0}", content.Id));
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var context = new BuildEditorContext(itemShape, content, groupId, _shapeFactory);
            BindPlacement(context, null, stereotype);

            _handlers.Value.Invoke(handler => {
                var key = String.Format("ContentEditor:{0}:{1}", content.Id, handler.GetType().FullName);
                _profiler.StepStart(key, String.Format("Content Editor: {0}", content.Id));
                handler.BuildEditor(context);
                _profiler.StepStop(key);
            }
                , Logger);

            _profiler.StepStop(contentKey);
            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, IUpdateModel updater, string groupInfoId) {
            var contentKey = "ContentUpdate:" + content.Id.ToString();
            _profiler.StepStart(contentKey, String.Format("Content Update: {0}", content.Id));
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = "Content";

            var actualShapeType = stereotype + "_Edit";

            dynamic itemShape = CreateItemShape(actualShapeType);
            itemShape.ContentItem = content.ContentItem;

            var theme = _workContextAccessor.GetContext().CurrentTheme;
            var shapeTable = _shapeTableLocator.Value.Lookup(theme.Id);

            var context = new UpdateEditorContext(itemShape, content, updater, groupInfoId, _shapeFactory, shapeTable);
            BindPlacement(context, null, stereotype);

            _handlers.Value.Invoke(handler => {
                var key = String.Format("ContentUpdate:{0}:{1}", content.Id, handler.GetType().FullName);
                _profiler.StepStart(key, String.Format("Content Update: {0}", handler.GetType().FullName));
                handler.UpdateEditor(context);
                _profiler.StepStop(key);

            }, Logger);

            _profiler.StepStop(contentKey);
            return context.Shape;
        }

        private dynamic CreateItemShape(string actualShapeType) {
            Func<dynamic> call = () => _shapeFactory.Create("ContentZone", Arguments.Empty());
            var zoneHoldingBehavior = new ZoneHoldingBehavior(call, _workContextAccessor.GetContext().Layout);
            return _shapeFactory.Create(actualShapeType, Arguments.Empty(), new[] { zoneHoldingBehavior });
        }

        private void BindPlacement(BuildShapeContext context, string displayType, string stereotype) {
            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

                var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);

                var theme = workContext.CurrentTheme;
                var shapeTable = _shapeTableLocator.Value.Lookup(theme.Id);

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

                    // define which location should be used if none placement is hit
                    descriptor.DefaultPlacement = defaultLocation;

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
