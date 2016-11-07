using System;
using System.Globalization;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.UI.Resources;

namespace Orchard.Projections.FilterEditors.Forms {
    public class NumericFilterForm : IFormProvider {
        public const string FormName = "NumericFilter";

        private readonly Work<IResourceManager> _resourceManager;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public NumericFilterForm(IShapeFactory shapeFactory, Work<IResourceManager> resourceManager) {
            _resourceManager = resourceManager;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "NumericFilter",
                        _Operator: Shape.SelectList(
                            Id: "operator", Name: "Operator",
                            Title: T("Operator"),
                            Size: 1,
                            Multiple: false
                            ),
                        _FieldSetSingle: Shape.FieldSet(
                            Id: "fieldset-single",
                            _Value: Shape.TextBox(
                                Id: "value", Name: "Value",
                                Title: T("Value"),
                                Classes: new[] { "tokenized" }
                                )
                            ),
                        _FieldSetMin: Shape.FieldSet(
                            Id: "fieldset-min",
                            _Min: Shape.TextBox(
                                Id: "min", Name: "Min",
                                Title: T("Min"),
                                Classes: new[] { "tokenized" }
                                )
                            ),
                        _FieldSetMax: Shape.FieldSet(
                            Id: "fieldset-max",
                            _Max: Shape.TextBox(
                                Id: "max", Name: "Max",
                                Title: T("Max"),
                                Classes: new[] { "tokenized" }
                                )
                            )
                    );

                    _resourceManager.Value.Require("script", "jQuery");
                    _resourceManager.Value.Include("script", "~/Modules/Orchard.Projections/Scripts/numeric-editor-filter.js", "~/Modules/Orchard.Projections/Scripts/numeric-editor-filter.js");

                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.LessThan), Text = T("Is less than").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.LessThanEquals), Text = T("Is less than or equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Equals), Text = T("Is equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.NotEquals), Text = T("Is not equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.GreaterThanEquals), Text = T("Is greater than or equal to").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.GreaterThan), Text = T("Is greater than").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.Between), Text = T("Is between").Text });
                    f._Operator.Add(new SelectListItem { Value = Convert.ToString(NumericOperator.NotBetween), Text = T("Is not between").Text });

                    return f;
                };

            context.Form(FormName, form);

        }

        public static Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState, string property) {

            var op = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(formState.Operator));

            decimal min, max;

            if (op == NumericOperator.Between || op == NumericOperator.NotBetween) {
                min = Decimal.Parse(Convert.ToString(formState.Min), CultureInfo.InvariantCulture);
                max = Decimal.Parse(Convert.ToString(formState.Max), CultureInfo.InvariantCulture);
            }
            else {
                min = max = Decimal.Parse(Convert.ToString(formState.Value), CultureInfo.InvariantCulture);
            }

            switch (op) {
                case NumericOperator.LessThan:
                    return x => x.Lt(property, max);
                case NumericOperator.LessThanEquals:
                    return x => x.Le(property, max);
                case NumericOperator.Equals:
                    if (min == max) {
                        return x => x.Eq(property, min);
                    }
                    return y => y.And(x => x.Ge(property, min), x => x.Le(property, max));
                case NumericOperator.NotEquals:
                    return min == max ? (Action<IHqlExpressionFactory>)(x => x.Not(y => y.Eq(property, min))) : (y => y.Or(x => x.Lt(property, min), x => x.Gt(property, max)));
                case NumericOperator.GreaterThan:
                    return x => x.Gt(property, min);
                case NumericOperator.GreaterThanEquals:
                    return x => x.Ge(property, min);
                case NumericOperator.Between:
                    return y => y.And(x => x.Ge(property, min), x => x.Le(property, max));
                case NumericOperator.NotBetween:
                    return y => y.Or(x => x.Lt(property, min), x => x.Gt(property, max));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static LocalizedString DisplayFilter(string fieldName, dynamic formState, Localizer T) {
            var op = (NumericOperator)Enum.Parse(typeof(NumericOperator), Convert.ToString(formState.Operator));
            string value = Convert.ToString(formState.Value);
            string min = Convert.ToString(formState.Min);
            string max = Convert.ToString(formState.Max);

            switch (op) {
                case NumericOperator.LessThan:
                    return T("{0} is less than {1}", fieldName, value);
                case NumericOperator.LessThanEquals:
                    return T("{0} is less than or equal to {1}", fieldName, value);
                case NumericOperator.Equals:
                    return T("{0} equals {1}", fieldName, value);
                case NumericOperator.NotEquals:
                    return T("{0} is not equal to {1}", fieldName, value);
                case NumericOperator.GreaterThan:
                    return T("{0} is greater than {1}", fieldName, value);
                case NumericOperator.GreaterThanEquals:
                    return T("{0} is greater than or equal to {1}", fieldName, value);
                case NumericOperator.Between:
                    return T("{0} is between {1} and {2}", fieldName, min, max);
                case NumericOperator.NotBetween:
                    return T("{0} is not between {1} and {2}", fieldName, min, max);
            }

            // should never be hit, but fail safe
            return new LocalizedString(fieldName);
        }
    }

    public enum NumericOperator {
        LessThan,
        LessThanEquals,
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanEquals,
        Between,
        NotBetween
    }
}