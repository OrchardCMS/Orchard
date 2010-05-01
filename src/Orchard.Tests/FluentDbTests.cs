using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using Orchard.Tests.Records;

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

            #endregion
        }

        [Test]
        public void CreatingSchemaForStatedClassesInTempFile() {
            var types = new Types(typeof (FooRecord), typeof (BarRecord));

            var sessionFactory = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile("temp"))
                .Mappings(m => m.AutoMappings.Add(AutoMap.Source(types)))
                .ExposeConfiguration(c => new SchemaExport(c).Create(false, true))
                .BuildSessionFactory();

            var session = sessionFactory.OpenSession();
            session.Save(new FooRecord {Name = "Hello"});
            session.Save(new BarRecord {Height = 3, Width = 4.5m});
            session.Close();

            session = sessionFactory.OpenSession();
            var foos = session.CreateCriteria<FooRecord>().List();
            Assert.That(foos.Count, Is.EqualTo(1));
            Assert.That(foos, Has.All.Property("Name").EqualTo("Hello"));
            session.Close();
        }

        [Test]
        public void InMemorySQLiteCanBeUsedInSessionFactory() {
            var sessionFactory = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory())
                .BuildSessionFactory();

            var session = sessionFactory.OpenSession();
            session.Close();
        }

        [Test]
        public void UsingDataUtilityToBuildSessionFactory() {
            var factory = DataUtility.CreateSessionFactory(typeof (FooRecord), typeof (BarRecord));

            var session = factory.OpenSession();
            var foo1 = new FooRecord {Name = "world"};
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