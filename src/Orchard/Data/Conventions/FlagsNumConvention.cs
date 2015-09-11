using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Data.Conventions {
    public class FlagsEnumConvention :
        IPropertyConvention,
        IPropertyConventionAcceptance {
        #region IPropertyConvention Members

        public void Apply(IPropertyInstance instance) {
            instance.CustomType(instance.Property.PropertyType);
        }

        #endregion

        #region IPropertyConventionAcceptance Members

        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria) {
            criteria.Expect(x => x.Property.PropertyType.IsEnum && (x.Property.PropertyType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0));
        }

        #endregion
    }
}
