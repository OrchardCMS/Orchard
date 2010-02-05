using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Data.Conventions;
using Orchard.Environment;
using Orchard.ContentManagement.Records;

namespace Orchard.Data {
    public class HackSessionLocator : ISessionLocator, IDisposable {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly ITransactionManager _transactionManager;
        private static ISessionFactory _sessionFactory;
        private ISession _session;

        public HackSessionLocator(ICompositionStrategy compositionStrategy, ITransactionManager transactionManager) {
            _compositionStrategy = compositionStrategy;
            _transactionManager = transactionManager;
        }

        private ISessionFactory BindSessionFactory() {
            // TEMP: a real scenario would call for a session factory locator 
            // that would eventually imply the need for configuration against one or more actual sources
            // and a means to enlist record types from active packages into correct session factory

            var hackPath = HostingEnvironment.MapPath("~/App_Data/hack.db");

            var database = SQLiteConfiguration.Standard.UsingFile(hackPath);

            var recordDescriptors = _compositionStrategy.GetRecordDescriptors();

            return _sessionFactory ??
                   Interlocked.CompareExchange(
                       ref _sessionFactory,
                       BuildSessionFactory(database, recordDescriptors), null) ?? _sessionFactory;

        }

        private static ISessionFactory BuildSessionFactory(IPersistenceConfigurer database, IEnumerable<RecordDescriptor> recordDescriptors) {
            return Fluently.Configure()
                .Database(database)
                .Mappings(m => m.AutoMappings.Add(CreatePersistenceModel(recordDescriptors)))
                .ExposeConfiguration(c => new SchemaUpdate(c).Execute(false /*script*/, true /*doUpdate*/))
                .BuildSessionFactory();
        }

        public static AutoPersistenceModel CreatePersistenceModel(IEnumerable<RecordDescriptor> recordDescriptors) {
            var types = recordDescriptors.Select(d => d.Type);
            return AutoMap.Source(new TypeSource(types))
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

        private class TypeSource : ITypeSource {
            private readonly IEnumerable<Type> _recordTypes;

            public TypeSource(IEnumerable<Type> recordTypes) {
                _recordTypes = recordTypes;
            }

            public IEnumerable<Type> GetTypes() {
                return _recordTypes;
            }
        }

        public ISession For(Type entityType) {
            var sessionFactory = BindSessionFactory();
            _transactionManager.Demand();
            return _session ?? Interlocked.CompareExchange(ref _session, sessionFactory.OpenSession(), null) ?? _session;
        }

        public void Dispose() {
            //if (_session != null) {
            //    //_session.Flush();
            //    _session.Close();
            //}
        }
    }
}