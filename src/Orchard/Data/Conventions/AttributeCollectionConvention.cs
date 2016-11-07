using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace Orchard.Data.Conventions {
    public abstract class AttributeCollectionConvention<T> : ICollectionConvention, ICollectionConventionAcceptance where T : Attribute {
        public void Accept(IAcceptanceCriteria<ICollectionInspector> criteria) {
            criteria.Expect(inspector => GetAttribute(inspector) != null);
        }

        public void Apply(ICollectionInstance instance) {
            Apply(GetAttribute(instance), instance);
        }

        protected abstract void Apply(T attribute, ICollectionInstance instance);

        private static T GetAttribute(ICollectionInspector inspector) {
            return Attribute.GetCustomAttribute(inspector.Member, typeof(T)) as T;
        }
    }
}