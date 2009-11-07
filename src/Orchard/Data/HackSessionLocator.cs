using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

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

                var automaps = new AutoPersistenceModel[] {
                    CreatePersistenceModel(Assembly.Load("Orchard.CmsPages")),
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

        private AutoPersistenceModel CreatePersistenceModel(Assembly assembly) {
            return AutoMap.Assembly(assembly)
                .Where(t => t.Namespace.EndsWith(".Models") && t.GetProperty("Id") != null)
                .Alterations(alt => alt.Add(new AutoMappingOverrideAlteration(assembly)))
                .Conventions.AddFromAssemblyOf<DataModule>();
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