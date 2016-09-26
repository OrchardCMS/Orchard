using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using NHibernate.Type;

namespace Orchard.Data.Conventions {
    public class UtcDateTimeConvention : IPropertyConvention, IPropertyConventionAcceptance {
        public void Apply(IPropertyInstance instance) {
            instance.CustomType<UtcDateTimeType>();
        }

        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria) {
            criteria.Expect(x =>
                x.Property.Name.EndsWith("Utc", StringComparison.OrdinalIgnoreCase) && (
                    x.Property.PropertyType.Equals(typeof(DateTime)) ||
                    x.Property.PropertyType.Equals(typeof(DateTime?))
                )
            );
        }
    }
}
