using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment;

namespace Orchard.Data.Conventions {
    public class RecordTableNameConvention : IClassConvention {
        private readonly IEnumerable<RecordDescriptor> _descriptors;

        public RecordTableNameConvention(IEnumerable<RecordDescriptor> descriptors) {
            _descriptors = descriptors;
        }

        public void Apply(IClassInstance instance) {
            var desc = _descriptors.Where(d => d.Type == instance.EntityType).SingleOrDefault();
            if (desc != null) {
                instance.Table(desc.Prefix + "_" + desc.Type.Name);
            }
        }
    }
}