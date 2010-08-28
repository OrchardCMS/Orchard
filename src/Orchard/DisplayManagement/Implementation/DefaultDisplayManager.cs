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
        private readonly IShapeTableFactory _shapeTableFactory;

        // this need to be Shape instead of IShape - cast to interface throws error on clr types like HtmlString
        private static readonly CallSite<Func<CallSite, object, Shape>> _convertAsShapeCallsite = CallSite<Func<CallSite, object, Shape>>.Create(
                new ForgivingConvertBinder(
                    (ConvertBinder)Binder.Convert(
                    CSharpBinderFlags.ConvertExplicit,
                    typeof(Shape),
                    null/*typeof(DefaultDisplayManager)*/)));

        public DefaultDisplayManager(IShapeTableManager shapeTableManager) {
            _shapeTableManager = shapeTableManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public IHtmlString Execute(DisplayContext context) {

            var shape = _convertAsShapeCallsite.Target(_convertAsShapeCallsite, context.Value);

            // non-shape arguements are returned as a no-op
            if (shape == null)
                return CoerceHtmlString(context.Value);

            var shapeAttributes = shape.Metadata;
            // can't really cope with a shape that has no type information
            if (shapeAttributes == null || string.IsNullOrEmpty(shapeAttributes.Type))
                return CoerceHtmlString(context.Value);

            var shapeTable = _shapeTableManager.GetShapeTable(null);
            //preproc loop / event (alter shape, swapping type)

            ShapeDescriptor shapeDescriptor;
            if (shapeTable.Descriptors.TryGetValue(shapeAttributes.Type, out shapeDescriptor)) {
                return Process(shapeDescriptor, shape, context);
            }
            //postproc / content html alteration/wrapping/etc
            throw new OrchardException(T("Shape type {0} not found", shapeAttributes.Type));
        }

        private IHtmlString CoerceHtmlString(object value) {
            if (value == null)
                return null;

            var result = value as IHtmlString;
            if (result != null)
                return result;

            return new HtmlString(HttpUtility.HtmlEncode(value));
        }

        private IHtmlString Process(ShapeDescriptor shapeDescriptor, IShape shape, DisplayContext context) {
            return CoerceHtmlString(shapeDescriptor.Binding(context));
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
                return _innerBinder.FallbackConvert(
                    target,
                    errorSuggestion ?? new DynamicMetaObject(Expression.Default(_innerBinder.ReturnType), GetTypeRestriction(target)));
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