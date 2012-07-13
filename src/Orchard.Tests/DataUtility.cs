using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Diagnostics;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Data;
using Orchard.Data.Providers;
using Orchard.Environment.ShellBuilders.Models;
using MsSqlCeConfiguration = Orchard.Data.Providers.MsSqlCeConfiguration;

namespace Orchard.Tests {
    public static class DataUtility {
        public static ISessionFactory CreateSessionFactory(string fileName, params Type[] types) {

            //var persistenceModel = AutoMap.Source(new Types(types))
            //    .Alterations(alt => AddAlterations(alt, types))
            //    .Conventions.AddFromAssemblyOf<DataModule>();
            var persistenceModel = AbstractDataServicesProvider.CreatePersistenceModel(types.Select(t => new RecordBlueprint { TableName = "Test_" + t.Name, Type = t }).ToList());
            var persistenceConfigurer = new SqlCeDataServicesProvider(fileName).GetPersistenceConfigurer(true/*createDatabase*/);
            ((MsSqlCeConfiguration)persistenceConfigurer).ShowSql();

            return Fluently.Configure()
                .Database(persistenceConfigurer)
                .Mappings(m => m.AutoMappings.Add(persistenceModel))
                .ExposeConfiguration(c => {
                    // This is to work around what looks to be an issue in the NHibernate driver:
                    // When inserting a row with IDENTITY column, the "SELET @@IDENTITY" statement
                    // is issued as a separate command. By default, it is also issued in a separate
                    // connection, which is not supported (returns NULL).
                    c.SetProperty("connection.release_mode", "on_close");
                    new SchemaExport(c).Create(false /*script*/, true /*export*/);
                })
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

            public void LogSource(IDiagnosticLogger logger) {
                throw new NotImplementedException();
            }

            public string GetIdentifier() {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }
}