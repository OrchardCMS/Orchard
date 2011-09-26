using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate.Cfg;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Providers {
    public abstract class AbstractDataServicesProvider : IDataServicesProvider {

        public abstract IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);

        public Configuration BuildConfiguration(SessionFactoryParameters parameters) {
            var database = GetPersistenceConfigurer(parameters.CreateDatabase);
            var persistenceModel = CreatePersistenceModel(parameters.RecordDescriptors);

            return Fluently.Configure()
                .Database(database)
                .Mappings(m => m.AutoMappings.Add(persistenceModel))
                .BuildConfiguration();
        }

        public static AutoPersistenceModel CreatePersistenceModel(IEnumerable<RecordBlueprint> recordDescriptors) {
            return AutoMap.Source(new TypeSource(recordDescriptors))
                // Ensure that namespaces of types are never auto-imported, so that 
                // identical type names from different namespaces can be mapped without ambiguity
                .Conventions.Setup(x => x.Add(AutoImport.Never()))
                .Conventions.Add(new RecordTableNameConvention(recordDescriptors))
                .Alterations(alt => {
                    foreach (var recordAssembly in recordDescriptors.Select(x => x.Type.Assembly).Distinct()) {
                        alt.Add(new AutoMappingOverrideAlteration(recordAssembly));
                    }
                    alt.AddFromAssemblyOf<DataModule>();
                    alt.Add(new ContentItemAlteration(recordDescriptors));
                })
                .Conventions.AddFromAssemblyOf<DataModule>();
        }

        class TypeSource : ITypeSource {
            private readonly IEnumerable<RecordBlueprint> _recordDescriptors;

            public TypeSource(IEnumerable<RecordBlueprint> recordDescriptors) { _recordDescriptors = recordDescriptors; }

            public IEnumerable<Type> GetTypes() { return _recordDescriptors.Select(descriptor => descriptor.Type); }
        }
    }
}