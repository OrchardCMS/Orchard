using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Conventions {
    public class CacheConvention : IClassConvention, IConventionAcceptance<IClassInspector> {
        private readonly IEnumerable<RecordBlueprint> _descriptors;

        public CacheConvention(IEnumerable<RecordBlueprint> descriptors) {
            _descriptors = descriptors;
        }

        public void Apply(IClassInstance instance) {
            instance.Cache.ReadWrite();
        }

        public void Accept(IAcceptanceCriteria<IClassInspector> criteria) {
            criteria.Expect(x => _descriptors.Any(d => d.Type.Name == x.EntityType.Name));
        }
    }
}