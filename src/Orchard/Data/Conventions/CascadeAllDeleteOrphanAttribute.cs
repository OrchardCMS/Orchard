using System;
using FluentNHibernate.Conventions.Instances;

namespace Orchard.Data.Conventions {

    public class CascadeAllDeleteOrphanAttribute : Attribute {
    }

    public class CascadeAllDeleteOrphanConvention : 
        AttributeCollectionConvention<CascadeAllDeleteOrphanAttribute> {

        protected override void Apply(CascadeAllDeleteOrphanAttribute attribute, ICollectionInstance instance) {
            instance.Cascade.AllDeleteOrphan();
        }
    }
}
