using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Tests {
    public static class DataUtility {
        public static ISessionFactory CreateSessionFactory(string fileName, params Type[] types) {

            //var persistenceModel = AutoMap.Source(new Types(types))
            //    .Alterations(alt => AddAlterations(alt, types))
            //    .Conventions.AddFromAssemblyOf<DataModule>();
            var persistenceModel = AbstractDataServicesProvider.CreatePersistenceModel(types.Select(t => new RecordBlueprint { TableName = "Test_" + t.Name,Type = t }));

            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(fileName).ShowSql())
                .Mappings(m => m.AutoMappings.Add(persistenceModel))
                .ExposeConfiguration(c => new SchemaExport(c).Create(false /*script*/, true /*export*/))
                .BuildSessionFactory();
        }

        private static void AddAlterations(AutoMappingAlterationCollection alterations, IEnumerable<Type> types) {
            foreach (var assembly in types.Select(t => t.Assembly).Distinct()) {
                alterations.Add(new AutoMappingOverrideAlteration(assembly));
                alterations.AddFromAssembly(assembly);
            }
            alterations.AddFromAssemblyOf<DataModule>();
        }

        public static ISessionFactory CreateSessionFactory(params Type[] types) {
            return CreateSessionFactory(
                types.Aggregate("db", (n, t) => t.FullName + "." + n),
                types);
        }

        #region Nested type: Types

        private class Types : ITypeSource {
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

        #endregion
    }
}