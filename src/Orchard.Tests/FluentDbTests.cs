using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Diagnostics;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using Orchard.Data.Providers;
using Orchard.Tests.Records;
using MsSqlCeConfiguration = Orchard.Data.Providers.MsSqlCeConfiguration;

namespace Orchard.Tests {
    [TestFixture]
    public class FluentDbTests {
        public class Types : ITypeSource {
            private readonly IEnumerable<Type> _types;

            public Types(params Type[] types) {
                _types = types;
            }

            #region ITypeSource Members

            public IEnumerable<Type> GetTypes() {
                return _types;
            }

            public void LogSource(IDiagnosticLogger logger) {
                throw new NotImplementedException();
            }

            public string GetIdentifier() {
                throw new NotImplementedException();
            }

            #endregion
        }

        [Test]
        public void CreatingSchemaForStatedClassesInTempFile() {
            var types = new Types(typeof(FooRecord), typeof(BarRecord));

            var fileName = "temp.sdf";
            var persistenceConfigurer = new SqlCeDataServicesProvider(fileName).GetPersistenceConfigurer(true/*createDatabase*/);
            ((MsSqlCeConfiguration)persistenceConfigurer).ShowSql();

            var sessionFactory = Fluently.Configure()
                .Database(persistenceConfigurer)
                .Mappings(m => m.AutoMappings.Add(AutoMap.Source(types)))
                .ExposeConfiguration(c => {
                    // This is to work around what looks to be an issue in the NHibernate driver:
                    // When inserting a row with IDENTITY column, the "SELET @@IDENTITY" statement
                    // is issued as a separate command. By default, it is also issued in a separate
                    // connection, which is not supported (returns NULL).
                    c.SetProperty("connection.release_mode", "on_close");
                    new SchemaExport(c).Create(false, true);
                })
                .BuildSessionFactory();

            var session = sessionFactory.OpenSession();
            session.Save(new FooRecord { Name = "Hello" });
            session.Save(new BarRecord { Height = 3, Width = 4.5m });
            session.Close();

            session = sessionFactory.OpenSession();
            var foos = session.CreateCriteria<FooRecord>().List();
            Assert.That(foos.Count, Is.EqualTo(1));
            Assert.That(foos, Has.All.Property("Name").EqualTo("Hello"));
            session.Close();
        }


        [Test]
        public void UsingDataUtilityToBuildSessionFactory() {
            var factory = DataUtility.CreateSessionFactory(typeof(FooRecord), typeof(BarRecord));

            var session = factory.OpenSession();
            var foo1 = new FooRecord { Name = "world" };
            session.Save(foo1);
            session.Close();

            session = factory.OpenSession();
            var foo2 = session.CreateCriteria<FooRecord>()
                .Add(Restrictions.Eq("Name", "world"))
                .List<FooRecord>().Single();
            session.Close();

            Assert.That(foo1, Is.Not.SameAs(foo2));
            Assert.That(foo1.Id, Is.EqualTo(foo2.Id));
        }
    }
}