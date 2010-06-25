using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Providers {
    public abstract class AbstractDataServicesProvider : IDataServicesProvider {

        protected abstract IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase);

        public ISessionFactory BuildSessionFactory(SessionFactoryParameters parameters) {
            var database = GetPersistenceConfigurer(parameters.CreateDatabase);
            var persistenceModel = CreatePersistenceModel(parameters.RecordDescriptors);

            var sessionFactory = Fluently.Configure()
                .Database(database)
                .Mappings(m => m.AutoMappings.Add(persistenceModel))
                .ExposeConfiguration(config => Initialization(parameters, config))
                .BuildSessionFactory();

            return sessionFactory;
        }

        private static void Initialization(SessionFactoryParameters parameters, Configuration configuration) {
            if (parameters.CreateDatabase) {
                var export = new SchemaExport(configuration);
                export.Execute(false/*script*/, true/*export*/, false/*justDrop*/);
            }
            else if (parameters.UpdateSchema) {
                var update = new SchemaUpdate(configuration);
                update.Execute(false/*script*/, true /*doUpdate*/);
            }
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