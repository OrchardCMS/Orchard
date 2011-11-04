using System;
using System.Collections;
using System.Collections.Generic;

namespace Orchard.ContentManagement {
    public interface IExpressionFactory {
        IExpressionFactory WithRecord(string recordName);
        IExpressionFactory WithVersionRecord(string recordName);
        IExpressionFactory WithRelationship(string propertyName);
        IExpressionFactory WithIds(ICollection<int> ids);

        void Eq(string propertyName, object value);
        void Like(string propertyName, object value);
        void Like(string propertyName, string value, MatchMode matchMode, char? escapeChar);
        void Like(string propertyName, string value, MatchMode matchMode);
        void InsensitiveLike(string propertyName, string value, MatchMode matchMode);
        void InsensitiveLike(string propertyName, object value);
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
        void And(Action<IExpressionFactory> lhs, Action<IExpressionFactory> rhs);
        void Or(Action<IExpressionFactory> lhs, Action<IExpressionFactory> rhs);
        void Not(Action<IExpressionFactory> expression);
        void Conjunction(Action<IExpressionFactory> expression, params Action<IExpressionFactory>[] otherExpressions);
        void Disjunction(Action<IExpressionFactory> expression, params Action<IExpressionFactory>[] otherExpressions);
        void AllEq(IDictionary propertyNameValues);
        void NaturalId();        
    }

    public enum MatchMode {
        Exact,
        Start,
        End,
        Anywhere
    }
}
