using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment;

namespace Orchard.Data.Conventions {
    public class RecordTableNameConvention : IClassConvention {
        private readonly IEnumerable<RecordDescriptor_Obsolete> _descriptors;

        public RecordTableNameConvention(IEnumerable<RecordDescriptor_Obsolete> descriptors) {
            _descriptors = descriptors;
        }

        public void Apply(IClassInstance instance) {
            var desc = _descriptors.Where(d => d.Type == instance.EntityType).SingleOrDefault();
            if (desc != null) {
                if (!string.IsNullOrEmpty(desc.Prefix)) {
                    instance.Table(desc.Prefix + "_" + desc.Type.Name);
                }
            }
        }
    }
}