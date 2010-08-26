using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;

namespace Orchard.DisplayManagement.Secondary {
    public class DefaultDisplayManager : IDisplayManager {
        private readonly IShapeTableFactory _shapeTableFactory;

        private static CallSite<Func<CallSite, object, IShape>> _convertAsShapeCallsite = CallSite<Func<CallSite, object, IShape>>.Create(
                new ForgivingConvertBinder(
                    (ConvertBinder)Binder.Convert(
                    CSharpBinderFlags.ConvertExplicit | CSharpBinderFlags.CheckedContext,
                    typeof(IShape),
                    null/*typeof(DefaultDisplayManager)*/)));

        public DefaultDisplayManager(IShapeTableFactory shapeTableFactory) {
            _shapeTableFactory = shapeTableFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public IHtmlString Execute(DisplayContext context) {
            var shape = _convertAsShapeCallsite.Target(_convertAsShapeCallsite, context.Value);

            // non-shape arguements are returned as a no-op
            if (shape == null)
                return CoerceHtmlString(context.Value);

            var shapeAttributes = shape.Attributes;
            // can't really cope with a shape that has no type information
            if (shapeAttributes == null || string.IsNullOrEmpty(shapeAttributes.Type))
                return CoerceHtmlString(context.Value);

            var shapeTable = _shapeTableFactory.CreateShapeTable();
            ShapeTable.Entry entry;
            if (shapeTable.Entries.TryGetValue(shapeAttributes.Type, out entry)) {
                return Process(entry, shape, context);
            }
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

        private IHtmlString Process(ShapeTable.Entry entry, IShape shape, DisplayContext context) {
            return CoerceHtmlString(entry.Target(context));
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