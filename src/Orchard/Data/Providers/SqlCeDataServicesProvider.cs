using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using FluentNHibernate.Cfg.Db;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.SqlTypes;

namespace Orchard.Data.Providers {
    public class SqlCeDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _fileName;
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SqlCeDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
            _fileName = Path.Combine(_dataFolder, "Orchard.sdf");
        }

        public SqlCeDataServicesProvider(string fileName) {
            _dataFolder = Path.GetDirectoryName(fileName);
            _fileName = fileName;
        }

        public static string ProviderName {
            get { return "SqlCe"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            var persistence = MsSqlCeConfiguration.MsSqlCe40;

            if (createDatabase) {
                File.Delete(_fileName);
            }

            string localConnectionString = string.Format("Data Source={0}", _fileName);
            if (!File.Exists(_fileName)) {
                CreateSqlCeDatabaseFile(localConnectionString);
            }

            persistence = persistence.ConnectionString(localConnectionString);
            persistence = persistence.Driver(typeof(OrchardSqlServerCeDriver).AssemblyQualifiedName);
            return persistence;
        }

        private void CreateSqlCeDatabaseFile(string connectionString) {
            if (!string.IsNullOrEmpty(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            // We want to execute this code using Reflection, to avoid having a binary
            // dependency on SqlCe assembly

            //engine engine = new SqlCeEngine();
            //const string assemblyName = "System.Data.SqlServerCe, Version=4.0.0.1, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
            const string assemblyName = "System.Data.SqlServerCe";
            const string typeName = "System.Data.SqlServerCe.SqlCeEngine";

            var sqlceEngineHandle = Activator.CreateInstance(assemblyName, typeName);
            var engine = sqlceEngineHandle.Unwrap();

            //engine.LocalConnectionString = connectionString;
            engine.GetType().GetProperty("LocalConnectionString").SetValue(engine, connectionString, null/*index*/);

            //engine.CreateDatabase();
            engine.GetType().GetMethod("CreateDatabase").Invoke(engine, null);

            //engine.Dispose();
            engine.GetType().GetMethod("Dispose").Invoke(engine, null);
        }

        public class OrchardSqlServerCeDriver : SqlServerCeDriver {
            private PropertyInfo _dbParamSqlDbTypeProperty;

            public override void Configure(IDictionary<string, string> settings) {
                base.Configure(settings);
                using ( var cmd = CreateCommand() ) {
                    var dbParam = cmd.CreateParameter();
                    _dbParamSqlDbTypeProperty = dbParam.GetType().GetProperty("SqlDbType");
                }
            }

            protected override void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType) {
                base.InitializeParameter(dbParam, name, sqlType);

                if(sqlType.DbType == DbType.Binary) {
                    _dbParamSqlDbTypeProperty.SetValue(dbParam, SqlDbType.Image, null);
                    return;
                }

                if ( sqlType.Length <= 4000 ) {
                    return;
                }

                switch(sqlType.DbType) {
                    case DbType.String:
                        _dbParamSqlDbTypeProperty.SetValue(dbParam, SqlDbType.NText, null);
                        break;
                    case DbType.AnsiString:
                        _dbParamSqlDbTypeProperty.SetValue(dbParam, SqlDbType.Text, null);
                        break;
                }
            }
        }
    }

    public class MsSqlCeConfiguration : PersistenceConfiguration<MsSqlCeConfiguration> {
        protected MsSqlCeConfiguration() {
            Driver<CustomSqlServerCeDriver>();
        }

        public static MsSqlCeConfiguration MsSqlCe40 {
            get { return new MsSqlCeConfiguration().Dialect<CustomMsSqlCe40Dialect>(); }

        }

        /// <summary>
        /// Custom driver so that Text/NText fields are not truncated at 4000 characters
        /// </summary>
        public class CustomSqlServerCeDriver : SqlServerCeDriver {
            protected override void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType) {
                base.InitializeParameter(dbParam, name, sqlType);

                PropertyInfo dbParamSqlDbTypeProperty = dbParam.GetType().GetProperty("SqlDbType");

                if (sqlType.Length <= 4000) {
                    return;
                }

                switch (sqlType.DbType) {
                    case DbType.String:
                        dbParamSqlDbTypeProperty.SetValue(dbParam, SqlDbType.NText, null);
                        break;
                    case DbType.AnsiString:
                        dbParamSqlDbTypeProperty.SetValue(dbParam, SqlDbType.Text, null);
                        break;
                }
            }
        }

        public class CustomMsSqlCe40Dialect : MsSqlCe40Dialect {
            public override bool SupportsVariableLimit {
                get { return true; }
            }
        }
    }
}
