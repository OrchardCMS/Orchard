using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Orchard.Data.Conventions
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LazyLoadAttribute : Attribute {
    }

    public class LazyLoadConvention : AttributePropertyConvention<LazyLoadAttribute> {
        protected override void Apply(LazyLoadAttribute attribute, IPropertyInstance instance) {
            instance.LazyLoad();
        }
    }
}