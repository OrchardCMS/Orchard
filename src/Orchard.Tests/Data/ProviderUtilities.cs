using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Autofac.Features.Metadata;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Orchard.Data.Providers;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Tests.Data {
    public class ProviderUtilities {

        public static void RunWithSqlServer(IEnumerable<RecordBlueprint> recordDescriptors, Action<ISessionFactory> action) {
            var temporaryPath = Path.GetTempFileName();
            if(File.Exists(temporaryPath))
                File.Delete(temporaryPath);
            Directory.CreateDirectory(temporaryPath);
            var databasePath = Path.Combine(temporaryPath, "Orchard.mdf");
            var databaseName = Path.GetFileNameWithoutExtension(databasePath);
            try {

                // create database
                using ( var connection = new SqlConnection(
                    "Data Source=.\\SQLEXPRESS;Initial Catalog=tempdb;Integrated Security=true;User Instance=True;") ) {
                    connection.Open();
                    using ( var command = connection.CreateCommand() ) {
                        command.CommandText =
                            "CREATE DATABASE " + databaseName +
                            " ON PRIMARY (NAME=" + databaseName +
                            ", FILENAME='" + databasePath.Replace("'", "''") + "')";
                        command.ExecuteNonQuery();

                        command.CommandText =
                            "EXEC sp_detach_db '" + databaseName + "', 'true'";
                        command.ExecuteNonQuery();
                    }
                }

                var manager = (IDataServicesProviderFactory)new DataServicesProviderFactory(new[] {
                    new Meta<CreateDataServicesProvider>(
                                                                                                 (dataFolder, connectionString) => new SqlServerDataServicesProvider(dataFolder, connectionString),
                                                                                                 new Dictionary<string, object> {{"ProviderName", "SqlServer"}})
                });
                var parameters = new SessionFactoryParameters {
                    Provider = "SqlServer",
                    DataFolder = temporaryPath,
                    ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFileName=" + databasePath + ";Integrated Security=True;User Instance=True;",
                    RecordDescriptors = recordDescriptors,
                };

                var configuration = manager
                    .CreateProvider(parameters)
                    .BuildConfiguration(parameters);

                new SchemaExport(configuration).Execute(false, true, false);

                using ( var sessionFactory = configuration.BuildSessionFactory() ) {
                    action(sessionFactory);
                }
            }
            finally {
                try {
                    Directory.Delete(temporaryPath, true);
                }
                catch (IOException) {}
            }
        }

        public static void RunWithSqlCe(IEnumerable<RecordBlueprint> recordDescriptors, Action<ISessionFactory> action) {
            var temporaryPath = Path.GetTempFileName();
            if ( File.Exists(temporaryPath) )
                File.Delete(temporaryPath);
            Directory.CreateDirectory(temporaryPath);
            var databasePath = Path.Combine(temporaryPath, "Orchard.mdf");
            var databaseName = Path.GetFileNameWithoutExtension(databasePath);
            var parameters = new SessionFactoryParameters {
                Provider = "SqlCe",
                DataFolder = temporaryPath,
                RecordDescriptors = recordDescriptors
            };
            try {
                var manager = (IDataServicesProviderFactory)new DataServicesProviderFactory(new[] {
                new Meta<CreateDataServicesProvider>(
                    (dataFolder, connectionString) => new SqlCeDataServicesProvider(dataFolder, connectionString),
                    new Dictionary<string, object> {{"ProviderName", "SqlCe"}})
            });

                var configuration = manager
                    .CreateProvider(parameters)
                    .BuildConfiguration(parameters);

                configuration.SetProperty("connection.release_mode", "on_close");

                new SchemaExport(configuration).Execute(false, true, false);

                using ( var sessionFactory = configuration.BuildSessionFactory() ) {
                    action(sessionFactory);
                }

            }
            finally {
                try {
                    Directory.Delete(temporaryPath, true);
                }
                catch (IOException) {}
            }
        }
    }
}
