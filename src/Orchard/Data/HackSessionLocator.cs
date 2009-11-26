using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Environment;
using Orchard.Models;
using Orchard.Models.Records;

namespace Orchard.Data {
    public class HackSessionLocator : ISessionLocator, IDisposable {
        private readonly ICompositionStrategy _compositionStrategy;
        private static ISessionFactory _sessionFactory;
        private ISession _session;

        public HackSessionLocator(ICompositionStrategy compositionStrategy) {
            _compositionStrategy = compositionStrategy;
        }

        private ISessionFactory BindSessionFactory() {
            // TEMP: a real scenario would call for a session factory locator 
            // that would eventually imply the need for configuration against one or more actual sources
            // and a means to enlist record types from active packages into correct session factory

            var database =
                SQLiteConfiguration.Standard.UsingFile(HttpContext.Current.Server.MapPath("~/App_Data/hack.db"));

            var recordTypes = _compositionStrategy.GetRecordTypes();

            return _sessionFactory ??
                   Interlocked.CompareExchange(
                       ref _sessionFactory,
                       BuildSessionFactory(database, recordTypes), null) ?? _sessionFactory;

        }

        private static ISessionFactory BuildSessionFactory(IPersistenceConfigurer database, IEnumerable<Type> recordTypes) {
            return Fluently.Configure()
                .Database(database)
                .Mappings(m => m.AutoMappings.Add(CreatePersistenceModel(recordTypes)))
                .ExposeConfiguration(c => new SchemaUpdate(c).Execute(false /*script*/, true /*doUpdate*/))
                .BuildSessionFactory();
        }

        public static AutoPersistenceModel CreatePersistenceModel(IEnumerable<Type> recordTypes) {
            return AutoMap.Source(new TypeSource(recordTypes))
                .Alterations(alt => {
                                 foreach (var recordAssembly in recordTypes.Select(x => x.Assembly).Distinct()) {
                                     alt.Add(new AutoMappingOverrideAlteration(recordAssembly));
                                 }
                                 alt.AddFromAssemblyOf<DataModule>();
                                 alt.Add(new ContentItemRecordAlteration(recordTypes));
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
            return _session ?? Interlocked.CompareExchange(ref _session, BindSessionFactory().OpenSession(), null) ?? _session;
        }

        public void Dispose() {
            if (_session != null) {
                _session.Flush();
                _session.Close();
            }
        }
    }
}