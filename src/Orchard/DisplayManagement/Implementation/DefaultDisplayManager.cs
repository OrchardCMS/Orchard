using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;

namespace Orchard.DisplayManagement.Implementation {
    public class DefaultDisplayManager : IDisplayManager {
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        // this need to be Shape instead of IShape - cast to interface throws error on clr types like HtmlString
        private static readonly CallSite<Func<CallSite, object, Shape>> _convertAsShapeCallsite = CallSite<Func<CallSite, object, Shape>>.Create(
                new ForgivingConvertBinder(
                    (ConvertBinder)Binder.Convert(
                    CSharpBinderFlags.ConvertExplicit,
                    typeof(Shape),
                    null/*typeof(DefaultDisplayManager)*/)));

        public DefaultDisplayManager(
            IShapeTableManager shapeTableManager,
            IWorkContextAccessor workContextAccessor) {
            _shapeTableManager = shapeTableManager;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public IHtmlString Execute(DisplayContext context) {

            var shape = _convertAsShapeCallsite.Target(_convertAsShapeCallsite, context.Value);

            // non-shape arguments are returned as a no-op
            if (shape == null)
                return CoerceHtmlString(context.Value);

            var shapeMetadata = shape.Metadata;
            // can't really cope with a shape that has no type information
            if (shapeMetadata == null || string.IsNullOrEmpty(shapeMetadata.Type))
                return CoerceHtmlString(context.Value);

            var workContext = _workContextAccessor.GetContext(context.ViewContext);
            var shapeTable = _shapeTableManager.GetShapeTable(workContext.CurrentTheme.ThemeName);
            //preproc loop / event (alter shape, swapping type)


            ShapeDescriptor shapeDescriptor;
            ShapeBinding shapeBinding;
            if (TryGetDescriptor(shapeMetadata.Type, shapeTable, out shapeDescriptor, out shapeBinding)) {
                shape.Metadata.ChildContent = Process(shapeDescriptor, shapeBinding, shape, context);
            }
            else {
                throw new OrchardException(T("Shape type {0} not found", shapeMetadata.Type));
            }

            foreach (var frameType in shape.Metadata.Wrappers) {
                ShapeDescriptor frameDescriptor;
                ShapeBinding frameBinding;
                if (TryGetDescriptor(frameType, shapeTable, out frameDescriptor, out frameBinding)) {
                    shape.Metadata.ChildContent = Process(frameDescriptor, frameBinding, shape, context);
                }
            }

            return shape.Metadata.ChildContent;
        }

        static bool TryGetDescriptor(string shapeType, ShapeTable shapeTable, out ShapeDescriptor shapeDescriptor, out ShapeBinding shapeBinding) {

            var shapeTypeScan = shapeType;
            for (; ; ) {
                if (shapeTable.Descriptors.TryGetValue(shapeTypeScan, out shapeDescriptor) &&
                    shapeDescriptor.Bindings.TryGetValue(shapeTypeScan, out shapeBinding)) {
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

        static IHtmlString Process(ShapeDescriptor shapeDescriptor, ShapeBinding shapeBinding, IShape shape, DisplayContext context) {

            if (shapeDescriptor == null || shapeBinding == null || shapeBinding.Binding == null) {
                //todo: create result from all child shapes
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