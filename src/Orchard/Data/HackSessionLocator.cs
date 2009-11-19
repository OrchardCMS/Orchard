using System;
using System.Reflection;
using System.Threading;
using System.Web;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Models;

namespace Orchard.Data {
    public class HackSessionLocator : ISessionLocator, IDisposable {
        private static ISessionFactory _sessionFactory;
        private ISession _session;

        public ISessionFactory SessionFactory {
            get {
                // TEMP: a real scenario would call for a session factory locator 
                // that would eventually imply the need for configuration against one or more actual sources
                // and a means to enlist record types from active packages into correct session factory

                var database =
                    SQLiteConfiguration.Standard.UsingFile(HttpContext.Current.Server.MapPath("~/App_Data/hack.db"));

                var automaps = new[] {
                    CreatePersistenceModel(Assembly.Load("Orchard.CmsPages")),
                    CreatePersistenceModel(Assembly.Load("Orchard.Users")),
                    CreatePersistenceModel(Assembly.Load("Orchard.Roles")),
                    CreatePersistenceModel(Assembly.Load("Orchard")),
                    CreatePersistenceModel(Assembly.Load("Orchard.Media")),
                    CreatePersistenceModel(Assembly.Load("Orchard.Core")),
                    CreatePersistenceModel(Assembly.Load("Orchard.Wikis")),
                };

                return _sessionFactory ??
                       Interlocked.CompareExchange(
                           ref _sessionFactory,
                           Fluently.Configure()
                               .Database(database)
                               .Mappings(m => {
                                   foreach (var automap in automaps) {
                                       m.AutoMappings.Add(automap);
                                   }
                               })
                               .ExposeConfiguration(
                               c => new SchemaUpdate(c).Execute(false /*script*/, true /*doUpdate*/))
                               .BuildSessionFactory(), null) ?? _sessionFactory;
            }
        }

        private static AutoPersistenceModel CreatePersistenceModel(Assembly assembly) {
            return AutoMap.Assembly(assembly)
                .Where(IsRecordType)
                .Alterations(alt => alt
                    .Add(new AutoMappingOverrideAlteration(assembly))
                    .AddFromAssemblyOf<DataModule>())
                .Conventions.AddFromAssemblyOf<DataModule>();
        }

        private static bool IsRecordType(Type type) {
            return (type.Namespace.EndsWith(".Models") || type.Namespace.EndsWith(".Records")) &&
                   type.GetProperty("Id") != null &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                   !typeof(IModel).IsAssignableFrom(type);
        }

        public ISession For(Type entityType) {
            return _session ?? Interlocked.CompareExchange(ref _session, SessionFactory.OpenSession(), null) ?? _session;
        }

        public void Dispose() {
            if (_session != null) {
                _session.Flush();
                _session.Close();
            }
        }
    }
}