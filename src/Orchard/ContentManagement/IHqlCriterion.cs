using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement {
    public interface IHqlCriterion {

        string ToHql(IAlias alias);
    }

    public abstract class HqlCriterion : IHqlCriterion {
        public abstract string ToHql(IAlias alias);
    }

    public class BinaryExpression : HqlCriterion {
        public BinaryExpression(string op, string propertyName, string value, Func<string, string> processPropertyName = null) {
            if(value == null) {
                throw new ArgumentNullException("value");
            }

            Op = op;
            PropertyName = propertyName;
            Value = value;
            ProcessPropertyName = processPropertyName;
        }
        
        public string Op { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public Func<string, string> ProcessPropertyName { get; set; }

        public override string ToHql(IAlias alias) {
            var processed = String.Concat(alias.Name, ".", PropertyName);
            if(ProcessPropertyName != null) {
                processed = ProcessPropertyName(processed);
            }
            return String.Concat(processed, " ", Op, " ", Value);
        }
    }

    public class CompositeHqlCriterion : HqlCriterion {
        public string Op { get; set; }
        public IList<IHqlCriterion> Criterions { get; private set; }

        public CompositeHqlCriterion(string op) {
            Op = op;
            Criterions = new List<IHqlCriterion>();
        }

        public CompositeHqlCriterion Add(IHqlCriterion criterion) {
            Criterions.Add(criterion);
            return this;
        }

        public override string ToHql(IAlias alias) {
            var sb = new StringBuilder();
            var first = true;
            foreach(var criterion in Criterions) {
                if(!first) {
                    sb.Append(Op).Append(" ");
                }
                else {
                    first = false;
                }

                sb.Append(criterion.ToHql(alias)).Append(" ");
            }

            return sb.ToString();
        }
    }

    public static class HqlRestrictions {

        public static IEnumerable<string> FormatValue(IEnumerable values, bool quoteStrings = true) {
            return from object value in values select FormatValue(value, quoteStrings);
        }

        public static string FormatValue(object value, bool quoteStrings = true) {
            var typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode) {
                case TypeCode.String:
                    if (quoteStrings) {
                        return String.Concat("'", Convert.ToString(value, CultureInfo.InvariantCulture), "'");
                    }
                    
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static IHqlCriterion AllEq(IDictionary propertyNameValues) {
            var conjuction = new CompositeHqlCriterion("and");

            foreach(string propertyName in propertyNameValues.Keys) {
                conjuction.Add(Eq(propertyName, propertyNameValues[propertyName]));
            }

            return conjuction;
        }

        public static IHqlCriterion And(IHqlCriterion lhs, IHqlCriterion rhs) {
            return new CompositeHqlCriterion("and")
                .Add(lhs)
                .Add(rhs);
        }

        public static IHqlCriterion Between(string propertyName, object lo, object hi) {
            return null;
        }

        public static CompositeHqlCriterion Conjunction() {
            return new CompositeHqlCriterion("and");
        }

        public static CompositeHqlCriterion Disjunction() {
            return new CompositeHqlCriterion("or");
        }

        public static IHqlCriterion Eq(string propertyName, object value) {
            return new BinaryExpression("=", propertyName, FormatValue(value));
        }

        public static IHqlCriterion EqProperty(string propertyName, string otherPropertyName) {
            return null;
        }

        public static IHqlCriterion Ge(string propertyName, object value) {
            return new BinaryExpression(">=", propertyName, FormatValue(value));
        }

        public static IHqlCriterion GeProperty(string propertyName, string otherPropertyName) {
            return null;
        }

        public static IHqlCriterion Gt(string propertyName, object value) {
            return new BinaryExpression(">", propertyName, FormatValue(value));
        }

        public static IHqlCriterion GtProperty(string propertyName, string otherPropertyName) {
            return null;
        }

        public static IHqlCriterion IdEq(object value) {
            return null;
        }

        public static IHqlCriterion In(string propertyName, ICollection values) {
            if (values.Count == 0) {
                throw new ArgumentException("Collection can't be empty", "values");
            }
            return new BinaryExpression("in", propertyName, "(" + String.Join(", ", FormatValue(values)) + ")");
        }

        public static IHqlCriterion In(string propertyName, object[] values) {
            if (values.Length == 0) {
                throw new ArgumentException("Collection can't be empty", "values");
            }
            return new BinaryExpression("in", propertyName, "(" + String.Join(", ", FormatValue(values)) + ")");
        }

        public static IHqlCriterion InG<T>(string propertyName, ICollection<T> values) {
            if (values.Count == 0) {
                throw new ArgumentException("Collection can't be empty", "values");
            }
            return new BinaryExpression("in", propertyName, "(" + String.Join(", ", FormatValue(values)) + ")");
        }

        public static IHqlCriterion InsensitiveLike(string propertyName, string value, HqlMatchMode matchMode) {
            var expression = Like(propertyName, value, matchMode);
            expression.ProcessPropertyName = x => String.Concat("lower(", x, ")");

            return expression;
        }

        public static IHqlCriterion IsEmpty(string propertyName) {
            return null;
        }

        public static IHqlCriterion IsNotEmpty(string propertyName) {
            return null;
        }

        public static IHqlCriterion IsNotNull(string propertyName) {
            return null;
        }

        public static IHqlCriterion IsNull(string propertyName) {
            return null;
        }

        public static IHqlCriterion Le(string propertyName, object value) {
            return new BinaryExpression("<=", propertyName, FormatValue(value));
        }

        public static IHqlCriterion LeProperty(string propertyName, string otherPropertyName) {
            return null;
        }

        public static BinaryExpression Like(string propertyName, string value, HqlMatchMode matchMode) {
            switch (matchMode) {
                case HqlMatchMode.Start:
                    value = "'" + value + "%'";
                    break;
                case HqlMatchMode.Exact:
                    break;
                case HqlMatchMode.Anywhere:
                    value = "'%" + value + "%'";
                    break;
                case HqlMatchMode.End:
                    value = "'%" + value + "'";
                    break;
            }

            return new BinaryExpression("like", propertyName, FormatValue((object)value, false));
        }

        public static IHqlCriterion Lt(string propertyName, object value) {
            return new BinaryExpression("<", propertyName, FormatValue(value));
        }

        public static IHqlCriterion LtProperty(string propertyName, string otherPropertyName) {
            return null;
        }

        public static IHqlCriterion NaturalId() {
            return null;
        }

        public static IHqlCriterion Not(IHqlCriterion expression) {
            return null;
        }

        public static IHqlCriterion NotEqProperty(string propertyName, string otherPropertyName) {
            return null;
        }

        public static IHqlCriterion Or(IHqlCriterion lhs, IHqlCriterion rhs) {
            return new CompositeHqlCriterion("or")
                .Add(lhs)
                .Add(rhs);
        }
    }
}
