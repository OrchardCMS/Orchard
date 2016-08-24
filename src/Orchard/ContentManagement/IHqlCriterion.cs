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
            if (value == null) {
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
            if (ProcessPropertyName != null) {
                processed = ProcessPropertyName(processed);
            }
            return String.Concat(processed, " ", Op, " ", Value);
        }
    }

    public class NotExpression : HqlCriterion {
        public IHqlCriterion Criterion { get; set; }

        public NotExpression(IHqlCriterion criterion) {
            Criterion = criterion;
        }

        public override string ToHql(IAlias alias) {
            return String.Concat("not ", Criterion.ToHql(alias));
        }
    }

    public class ComplexExpression : HqlCriterion {
        public ComplexExpression(string op1, string op2, string propertyName, string value1, string value2) {
            if(value1 == null) {
                throw new ArgumentNullException("value1");
            }

            if(value2 == null) {
                throw new ArgumentNullException("value2");
            }

            Op1 = op1;
            Op2 = op2;
            PropertyName = propertyName;
            Value1 = value1;
            Value2 = value2;
        }
        
        public string Op1 { get; set; }
        public string Op2 { get; set; }
        public string PropertyName { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }

        public override string ToHql(IAlias alias) {
            var processed = String.Concat(alias.Name, ".", PropertyName);
            return String.Concat(processed, " ", Op1, " ", Value1, " ", Op2, " ", Value2);
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

        public static string FormatValue(string value, bool quoteStrings = true) {
            return FormatValue((object)value, quoteStrings);
        }

        public static string FormatValue(object value, bool quoteStrings = true) {
            var typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode) {
                case TypeCode.String:
                    if (quoteStrings) {
                        return String.Concat("'", EncodeQuotes(Convert.ToString(value, CultureInfo.InvariantCulture)), "'");
                    }

                    return EncodeQuotes(Convert.ToString(value, CultureInfo.InvariantCulture));
                case TypeCode.DateTime:
                    // convert the date time to a valid string representation for Hql (ISO 8601 format, which is language neutral)
                    var sortableDateTime = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    return quoteStrings ? String.Concat("'", EncodeQuotes(sortableDateTime), "'") : sortableDateTime;
            }

            return EncodeQuotes(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        private static string EncodeQuotes(string value) {
            return value.Replace("'", "''");
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
            return new ComplexExpression("between", "and", propertyName, FormatValue(lo), FormatValue(hi));
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
            return new BinaryExpression("=", propertyName, otherPropertyName);
        }

        public static IHqlCriterion Ge(string propertyName, object value) {
            return new BinaryExpression(">=", propertyName, FormatValue(value));
        }

        public static IHqlCriterion GeProperty(string propertyName, string otherPropertyName) {
            return new BinaryExpression(">=", propertyName, otherPropertyName);
        }

        public static IHqlCriterion Gt(string propertyName, object value) {
            return new BinaryExpression(">", propertyName, FormatValue(value));
        }

        public static IHqlCriterion GtProperty(string propertyName, string otherPropertyName) {
            return new BinaryExpression(">", propertyName, otherPropertyName);
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
            return new BinaryExpression("is", propertyName, "empty");
        }

        public static IHqlCriterion IsNotEmpty(string propertyName) {
            return new BinaryExpression("is", propertyName, "not empty");
        }

        public static IHqlCriterion IsNotNull(string propertyName) {
            return new BinaryExpression("is", propertyName, "not null");
        }

        public static IHqlCriterion IsNull(string propertyName) {
            return new BinaryExpression("is", propertyName, "null");
        }

        public static IHqlCriterion Le(string propertyName, object value) {
            return new BinaryExpression("<=", propertyName, FormatValue(value));
        }

        public static IHqlCriterion LeProperty(string propertyName, string otherPropertyName) {
            return new BinaryExpression("<=", propertyName, otherPropertyName);
        }

        public static BinaryExpression Like(string propertyName, string value, HqlMatchMode matchMode) {
            switch (matchMode) {
                case HqlMatchMode.Start:
                    value = "'" + FormatValue(value, false) + "%'";
                    break;
                case HqlMatchMode.Exact:
                    value = "'" + FormatValue(value, false) + "'";
                    break;
                case HqlMatchMode.Anywhere:
                    value = "'%" + FormatValue(value, false) + "%'";
                    break;
                case HqlMatchMode.End:
                    value = "'%" + FormatValue(value, false) + "'";
                    break;
            }

            return new BinaryExpression("like", propertyName, value);
        }

        public static IHqlCriterion Lt(string propertyName, object value) {
            return new BinaryExpression("<", propertyName, FormatValue(value));
        }

        public static IHqlCriterion LtProperty(string propertyName, string otherPropertyName) {
            return new BinaryExpression("<", propertyName, otherPropertyName);
        }

        public static IHqlCriterion NaturalId() {
            return null;
        }

        public static IHqlCriterion Not(IHqlCriterion expression) {
            return new NotExpression(expression);
        }

        public static IHqlCriterion NotEqProperty(string propertyName, string otherPropertyName) {
            return new BinaryExpression("!=", propertyName, otherPropertyName);
        }

        public static IHqlCriterion Or(IHqlCriterion lhs, IHqlCriterion rhs) {
            return new CompositeHqlCriterion("or")
                .Add(lhs)
                .Add(rhs);
        }
    }
}
