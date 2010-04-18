using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;

namespace Orchard.Data.Conventions {
    public class RecordTableNameConvention : IClassConvention {
        private readonly IEnumerable<RecordTopology> _descriptors;

        public RecordTableNameConvention(IEnumerable<RecordTopology> descriptors) {
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