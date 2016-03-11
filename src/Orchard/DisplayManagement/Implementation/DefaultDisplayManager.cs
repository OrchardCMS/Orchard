using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;

namespace Orchard.DisplayManagement.Implementation {
    public class DefaultDisplayManager : IDisplayManager {
        private readonly Lazy<IShapeTableLocator> _shapeTableLocator;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IEnumerable<IShapeDisplayEvents> _shapeDisplayEvents;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IShapeBindingResolver> _shapeBindingResolvers;

        // this need to be Shape instead of IShape - cast to interface throws error on clr types like HtmlString
        private static readonly CallSite<Func<CallSite, object, Shape>> _convertAsShapeCallsite = CallSite<Func<CallSite, object, Shape>>.Create(
                new ForgivingConvertBinder(
                    (ConvertBinder)Binder.Convert(
                    CSharpBinderFlags.ConvertExplicit,
                    typeof(Shape),
                    null/*typeof(DefaultDisplayManager)*/)));

        public DefaultDisplayManager(
            IWorkContextAccessor workContextAccessor,
            IEnumerable<IShapeDisplayEvents> shapeDisplayEvents,
            IEnumerable<IShapeBindingResolver> shapeBindingResolvers,
            IHttpContextAccessor httpContextAccessor,
            Lazy<IShapeTableLocator> shapeTableLocator) {
            _shapeTableLocator = shapeTableLocator;
            _workContextAccessor = workContextAccessor;
            _shapeDisplayEvents = shapeDisplayEvents;
            _httpContextAccessor = httpContextAccessor;
            _shapeBindingResolvers = shapeBindingResolvers;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IHtmlString Execute(DisplayContext context) {

            var shape = _convertAsShapeCallsite.Target(_convertAsShapeCallsite, context.Value);

            // non-shape arguments are returned as a no-op
            if (shape == null)
                return CoerceHtmlString(context.Value);

            var shapeMetadata = shape.Metadata;
            // can't really cope with a shape that has no type information
            if (shapeMetadata == null || string.IsNullOrEmpty(shapeMetadata.Type))
                return CoerceHtmlString(context.Value);

            var workContext = _workContextAccessor.GetContext();
            var shapeTable = !_httpContextAccessor.Current().IsBackgroundContext()
                ? _shapeTableLocator.Value.Lookup(workContext.CurrentTheme.Id)
                : _shapeTableLocator.Value.Lookup(null);

            var displayingContext = new ShapeDisplayingContext {
                Shape = shape,
                ShapeMetadata = shapeMetadata
            };
            _shapeDisplayEvents.Invoke(sde => sde.Displaying(displayingContext), Logger);

            // find base shape association using only the fundamental shape type. 
            // alternates that may already be registered do not affect the "displaying" event calls
            ShapeBinding shapeBinding;
            if (TryGetDescriptorBinding(shapeMetadata.Type, Enumerable.Empty<string>(), shapeTable, out shapeBinding)) {
                shapeBinding.ShapeDescriptor.Displaying.Invoke(action => action(displayingContext), Logger);

                // copy all binding sources (all templates for this shape) in order to use them as Localization scopes
                shapeMetadata.BindingSources = shapeBinding.ShapeDescriptor.BindingSources.Where(x => x != null).ToList();
                if (!shapeMetadata.BindingSources.Any()) {
                    shapeMetadata.BindingSources.Add(shapeBinding.ShapeDescriptor.BindingSource);
                }
            }

            // invoking ShapeMetadata displaying events
            shapeMetadata.Displaying.Invoke(action => action(displayingContext), Logger);

            // use pre-fectched content if available (e.g. coming from specific cache implmentation)
            if ( displayingContext.ChildContent != null ) {
                shape.Metadata.ChildContent = displayingContext.ChildContent;
            }
            else {
                // now find the actual binding to render, taking alternates into account
                ShapeBinding actualBinding;
                if ( TryGetDescriptorBinding(shapeMetadata.Type, shapeMetadata.Alternates, shapeTable, out actualBinding) ) {
                    shape.Metadata.ChildContent = Process(actualBinding, shape, context);
                }
                else {
                    throw new OrchardException(T("Shape type {0} not found", shapeMetadata.Type));
                }
            }

            foreach (var frameType in shape.Metadata.Wrappers) {
                ShapeBinding frameBinding;
                if (TryGetDescriptorBinding(frameType, Enumerable.Empty<string>(), shapeTable, out frameBinding)) {
                    shape.Metadata.ChildContent = Process(frameBinding, shape, context);
                }
            }

            var displayedContext = new ShapeDisplayedContext {
                Shape = shape,
                ShapeMetadata = shape.Metadata,
                ChildContent = shape.Metadata.ChildContent,
            };

            _shapeDisplayEvents.Invoke(sde => {
                var prior = displayedContext.ChildContent = displayedContext.ShapeMetadata.ChildContent;
                sde.Displayed(displayedContext);
                // update the child content if the context variable has been reassigned
                if (prior != displayedContext.ChildContent)
                    displayedContext.ShapeMetadata.ChildContent = displayedContext.ChildContent;
            }, Logger);

            if (shapeBinding != null) {
                shapeBinding.ShapeDescriptor.Displayed.Invoke(action => {
                    var prior = displayedContext.ChildContent = displayedContext.ShapeMetadata.ChildContent;
                    action(displayedContext);
                    // update the child content if the context variable has been reassigned
                    if (prior != displayedContext.ChildContent)
                        displayedContext.ShapeMetadata.ChildContent = displayedContext.ChildContent;
                }, Logger);
            }

            // invoking ShapeMetadata displayed events
            shapeMetadata.Displayed.Invoke(action => action(displayedContext), Logger);

            return shape.Metadata.ChildContent;
        }

        private bool TryGetDescriptorBinding(string shapeType, IEnumerable<string> shapeAlternates, ShapeTable shapeTable, out ShapeBinding shapeBinding) {
            // shape alternates are optional, fully qualified binding names
            // the earliest added alternates have the lowest priority
            // the descriptor returned is based on the binding that is matched, so it may be an entirely
            // different descriptor if the alternate has a different base name
            foreach (var shapeAlternate in shapeAlternates.Reverse()) {

                foreach (var shapeBindingResolver in _shapeBindingResolvers) {
                    if(shapeBindingResolver.TryGetDescriptorBinding(shapeAlternate, out shapeBinding)) {
                        return true;
                    }
                }

                if (shapeTable.Bindings.TryGetValue(shapeAlternate, out shapeBinding)) {
                    return true;
                }
            }

            // when no alternates match, the shapeType is used to find the longest matching binding
            // the shapetype name can break itself into shorter fallbacks at double-underscore marks
            // so the shapetype itself may contain a longer alternate forms that falls back to a shorter one
            var shapeTypeScan = shapeType;
            for (; ; ) {
                foreach (var shapeBindingResolver in _shapeBindingResolvers) {
                    if (shapeBindingResolver.TryGetDescriptorBinding(shapeTypeScan, out shapeBinding)) {
                        return true;
                    }
                }

                if (shapeTable.Bindings.TryGetValue(shapeTypeScan, out shapeBinding)) {
                    return true;
                }

                var delimiterIndex = shapeTypeScan.LastIndexOf("__");
                if (delimiterIndex < 0) {
                    shapeBinding = null;
                    return false;
                }

                shapeTypeScan = shapeTypeScan.Substring(0, delimiterIndex);
            }
        }

        static IHtmlString CoerceHtmlString(object value) {
            if (value == null)
                return null;

            var result = value as IHtmlString;
            if (result != null)
                return result;

            return new HtmlString(HttpUtility.HtmlEncode(value));
        }

        static IHtmlString Process(ShapeBinding shapeBinding, IShape shape, DisplayContext context) {

            if (shapeBinding == null || shapeBinding.Binding == null) {
                // todo: create result from all child shapes
                return shape.Metadata.ChildContent ?? new HtmlString("");
            }
            return CoerceHtmlString(shapeBinding.Binding(context));
        }

        class ForgivingConvertBinder : ConvertBinder {
            private readonly ConvertBinder _innerBinder;

            public ForgivingConvertBinder(ConvertBinder innerBinder)
                : base(innerBinder.ReturnType, innerBinder.Explicit) {
                _innerBinder = innerBinder;
            }

            public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                // adjust the normal csharp convert binder to allow failure to become null.
                // this causes the same net effect as the "as" keyword, but may be applied to dynamic objects
                var result = _innerBinder.FallbackConvert(
                    target,
                    errorSuggestion ?? new DynamicMetaObject(Expression.Default(_innerBinder.ReturnType), GetTypeRestriction(target)));
                return result;
            }

            static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj) {
                if ((obj.Value == null) && obj.HasValue) {
                    return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
                }
                return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
            }
        }
    }
}
