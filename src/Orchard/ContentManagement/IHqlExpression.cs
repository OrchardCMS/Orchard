using System;
using System.Collections;
using System.Collections.Generic;

namespace Orchard.ContentManagement {

    public interface IHqlExpressionFactory {

        void Eq(string propertyName, object value);
        void Like(string propertyName, string value, HqlMatchMode matchMode);
        void InsensitiveLike(string propertyName, string value, HqlMatchMode matchMode);
        void Gt(string propertyName, object value);
        void Lt(string propertyName, object value);
        void Le(string propertyName, object value);
        void Ge(string propertyName, object value);
        void Between(string propertyName, object lo, object hi);
        void In(string propertyName, object[] values);
        void In(string propertyName, ICollection values);
        void InG<T>(string propertyName, ICollection<T> values);
        void IsNull(string propertyName);
        void EqProperty(string propertyName, string otherPropertyName);
        void NotEqProperty(string propertyName, string otherPropertyName);
        void GtProperty(string propertyName, string otherPropertyName);
        void GeProperty(string propertyName, string otherPropertyName);
        void LtProperty(string propertyName, string otherPropertyName);
        void LeProperty(string propertyName, string otherPropertyName);
        void IsNotNull(string propertyName);
        void IsNotEmpty(string propertyName);
        void IsEmpty(string propertyName);
        void And(Action<IHqlExpressionFactory> lhs, Action<IHqlExpressionFactory> rhs);
        void Or(Action<IHqlExpressionFactory> lhs, Action<IHqlExpressionFactory> rhs);
        void Not(Action<IHqlExpressionFactory> expression);
        void Conjunction(Action<IHqlExpressionFactory> expression, params Action<IHqlExpressionFactory>[] otherExpressions);
        void Disjunction(Action<IHqlExpressionFactory> expression, params Action<IHqlExpressionFactory>[] otherExpressions);
        void AllEq(IDictionary propertyNameValues);
        void NaturalId();
    }

}
