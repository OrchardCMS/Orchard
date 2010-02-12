using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using Orchard.Environment;

namespace Orchard.Data.Migrations {
    public abstract class DatabaseCoordinatorBase : IDatabaseCoordinator {
        protected abstract IPersistenceConfigurer GetPersistenceConfigurer();

        public virtual bool CanConnect() {
            try {
                var sessionFactory = Fluently.Configure()
                    .Database(GetPersistenceConfigurer())
                    .BuildSessionFactory();
                try {
                    // attempting to open a session validates a connection can be made
                    var session = sessionFactory.OpenSession();
                    session.Close();
                }
                finally {
                    sessionFactory.Close();
                }
                return true;
            }
            catch {
                return false;
            }
        }

        public virtual void CreateDatabase() {
            // creating a session factory appears to be sufficient for causing a database file to be created for inplace providers
            var sessionFactory = Fluently.Configure()
                .Database(GetPersistenceConfigurer())
                .BuildSessionFactory();
            sessionFactory.Close();
        }

        public void UpdateSchema(IEnumerable<RecordDescriptor> recordDescriptors) {
            var configuration = Fluently.Configure()
                .Database(GetPersistenceConfigurer())
                .Mappings(m => m.AutoMappings.Add(CreatePersistenceModel(recordDescriptors)))
                .BuildConfiguration();

            var updater = new SchemaUpdate(configuration);
            updater.Execute(true /*script*/, true /*doUpdate*/);
        }

        public ISessionFactory BuildSessionFactory(IEnumerable<RecordDescriptor> recordDescriptors) {
            return Fluently.Configure()
                .Database(GetPersistenceConfigurer())
                .Mappings(m => m.AutoMappings.Add(CreatePersistenceModel(recordDescriptors)))
                .BuildSessionFactory();
        }

        static AutoPersistenceModel CreatePersistenceModel(IEnumerable<RecordDescriptor> recordDescriptors) {
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
            private readonly IEnumerable<RecordDescriptor> _recordDescriptors;

            public TypeSource(IEnumerable<RecordDescriptor> recordDescriptors) { _recordDescriptors = recordDescriptors; }

            public IEnumerable<Type> GetTypes() { return _recordDescriptors.Select(descriptor => descriptor.Type); }
        }
    }
}