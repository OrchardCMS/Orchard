using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Conventions {
    public class CacheConventions :
        IClassConvention, IClassConventionAcceptance,
        ICollectionConvention, ICollectionConventionAcceptance,
        IHasManyConvention, IHasManyConventionAcceptance,
        IHasManyToManyConvention, IHasManyToManyConventionAcceptance {

        private readonly IEnumerable<RecordBlueprint> _descriptors;

        public CacheConventions(IEnumerable<RecordBlueprint> descriptors) {
            _descriptors = descriptors;
        }


        public void Apply(IClassInstance instance) {
            instance.Cache.ReadWrite();
        }

        public void Accept(IAcceptanceCriteria<IClassInspector> criteria) {
            criteria.Expect(x => _descriptors.Any(d => d.Type.Name == x.EntityType.Name));
        }


        public void Apply(IOneToManyCollectionInstance instance) {
            instance.Cache.ReadWrite();
        }

        public void Accept(IAcceptanceCriteria<IOneToManyCollectionInspector> criteria) {
            criteria.Expect(x => _descriptors.Any(d => d.Type.Name == x.EntityType.Name));
        }


        public void Apply(IManyToManyCollectionInstance instance) {
            instance.Cache.ReadWrite();
        }

        public void Accept(IAcceptanceCriteria<IManyToManyCollectionInspector> criteria) {
            criteria.Expect(x => _descriptors.Any(d => d.Type.Name == x.EntityType.Name));
        }


        public void Apply(ICollectionInstance instance) {
            instance.Cache.ReadWrite();
        }

        public void Accept(IAcceptanceCriteria<ICollectionInspector> criteria) {
            criteria.Expect(x => _descriptors.Any(d => d.Type.Name == x.EntityType.Name));
        }
    }
}