using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Conventions {
    public class RecordTableNameConvention : IClassConvention {
        private readonly IEnumerable<RecordBlueprint> _descriptors;

        public RecordTableNameConvention(IEnumerable<RecordBlueprint> descriptors) {
            _descriptors = descriptors;
        }

        public void Apply(IClassInstance instance) {
            var desc = _descriptors.Where(d => d.Type == instance.EntityType).SingleOrDefault();
            if (desc != null) {
                instance.Table(desc.TableName);
            }
        }
    }
}