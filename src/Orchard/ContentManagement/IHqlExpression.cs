using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public class DefaultHqlExpressionFactory : IHqlExpressionFactory {
        public IHqlCriterion Criterion { get; private set; }

        public void Eq(string propertyName, object value) {
            Criterion = HqlRestrictions.Eq(propertyName, value);
        }

        public void Like(string propertyName, string value, HqlMatchMode matchMode) {
            Criterion = HqlRestrictions.Like(propertyName, value, matchMode);
        }

        public void InsensitiveLike(string propertyName, string value, HqlMatchMode matchMode) {
            Criterion = HqlRestrictions.InsensitiveLike(propertyName, value, matchMode);
        }

        public void Gt(string propertyName, object value) {
            Criterion = HqlRestrictions.Gt(propertyName, value);
        }

        public void Lt(string propertyName, object value) {
            Criterion = HqlRestrictions.Lt(propertyName, value);
        }

        public void Le(string propertyName, object value) {
            Criterion = HqlRestrictions.Le(propertyName, value);
        }

        public void Ge(string propertyName, object value) {
            Criterion = HqlRestrictions.Ge(propertyName, value);
        }

        public void Between(string propertyName, object lo, object hi) {
            Criterion = HqlRestrictions.Between(propertyName, lo, hi);
        }

        public void In(string propertyName, object[] values) {
            Criterion = HqlRestrictions.In(propertyName, values);
        }

        public void In(string propertyName, ICollection values) {
            Criterion = HqlRestrictions.In(propertyName, values);
        }

        public void InG<T>(string propertyName, ICollection<T> values) {
            Criterion = HqlRestrictions.InG(propertyName, values);
        }

        public void IsNull(string propertyName) {
            Criterion = HqlRestrictions.IsNull(propertyName);
        }

        public void EqProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.EqProperty(propertyName, otherPropertyName);
        }

        public void NotEqProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.NotEqProperty(propertyName, otherPropertyName);
        }

        public void GtProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.GtProperty(propertyName, otherPropertyName);
        }

        public void GeProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.GeProperty(propertyName, otherPropertyName);
        }

        public void LtProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.LtProperty(propertyName, otherPropertyName);
        }

        public void LeProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.LeProperty(propertyName, otherPropertyName);
        }

        public void IsNotNull(string propertyName) {
            Criterion = HqlRestrictions.IsNotNull(propertyName);
        }

        public void IsNotEmpty(string propertyName) {
            Criterion = HqlRestrictions.IsNotEmpty(propertyName);
        }

        public void IsEmpty(string propertyName) {
            Criterion = HqlRestrictions.IsEmpty(propertyName);
        }

        public void And(Action<IHqlExpressionFactory> lhs, Action<IHqlExpressionFactory> rhs) {
            lhs(this);
            var a = Criterion;
            rhs(this);
            var b = Criterion;
            Criterion = HqlRestrictions.And(a, b);
        }

        public void Or(Action<IHqlExpressionFactory> lhs, Action<IHqlExpressionFactory> rhs) {
            lhs(this);
            var a = Criterion;
            rhs(this);
            var b = Criterion;
            Criterion = HqlRestrictions.Or(a, b);
        }

        public void Not(Action<IHqlExpressionFactory> expression) {
            expression(this);
            var a = Criterion;
            Criterion = HqlRestrictions.Not(a);
        }

        public void Conjunction(Action<IHqlExpressionFactory> expression, params Action<IHqlExpressionFactory>[] otherExpressions) {
            var junction = HqlRestrictions.Conjunction();
            foreach (var exp in Enumerable.Empty<Action<IHqlExpressionFactory>>().Union(new[] { expression }).Union(otherExpressions)) {
                exp(this);
                junction.Add(Criterion);
            }

            Criterion = junction;
        }

        public void Disjunction(Action<IHqlExpressionFactory> expression, params Action<IHqlExpressionFactory>[] otherExpressions) {
            var junction = HqlRestrictions.Disjunction();
            foreach (var exp in Enumerable.Empty<Action<IHqlExpressionFactory>>().Union(new[] { expression }).Union(otherExpressions)) {
                exp(this);
                junction.Add(Criterion);
            }

            Criterion = junction;
        }

        public void AllEq(IDictionary propertyNameValues) {
            Criterion = HqlRestrictions.AllEq(propertyNameValues);
        }

        public void NaturalId() {
            Criterion = HqlRestrictions.NaturalId();
        }
    }

    public enum HqlMatchMode {
        Exact,
        Start,
        End,
        Anywhere
    }

}
