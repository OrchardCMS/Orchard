using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.SqlTypes;

namespace Orchard.Data.Providers {



    public class PostgreSqlDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public PostgreSqlDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName {
            get { return "PostgreSql"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            var persistence = PostgreSql82Configuration.PostgreSql82;
            if (string.IsNullOrEmpty(_connectionString)) {
                throw new ArgumentException("The connection string is empty");
            }
            persistence = persistence.ConnectionString(_connectionString);
            return persistence;
        }

        sealed class PostgreSql82DialectFixed : PostgreSQL82Dialect {

            // Works around a bug in NHibernate PostgreSQL82 dialect which overrides the 
            // GetIdentityColumnString method but fails to override the IdentityColumnString,
            // which eventually leads to exception being thrown.
            public override string IdentityColumnString {
                get { return "serial"; }
            }

            // Avoid to quote any identifiers for PostgreSQL. Doing that will fold all of them into
            // lower case which will then make it easier to issue queries. When an identifier (e.g.
            // a table name) is enclosed in quotes when creating it you have to always use quotes
            // _and_ the correct case when referring to it thereafter.
            protected override string Quote(string name) {
                return name;
            }

            // PostgreSQL does not accept the default Dialect's 0 or 1 value for boolean columns.
            public override string ToBooleanValueString(bool value) {
                return value ? "'t'" : "'f'";
            }

            public override string QuoteForColumnName(string columnName) {
                return columnName;
            }

            public override string QuoteForTableName(string tableName) {
                return tableName;
            }
        }

        sealed class PostgreSql82Configuration : PersistenceConfiguration<PostgreSql82Configuration> {

            public static PostgreSql82Configuration PostgreSql82 {
                get { return new PostgreSql82Configuration().Dialect<PostgreSql82DialectFixed>(); }

            }

        }

    }

    public class PostgreSqlNamingStrategy : INamingStrategy {
        public string ClassToTableName(string className) {
            return DoubleQuote(className);
        }
        public string PropertyToColumnName(string propertyName) {
            return DoubleQuote(propertyName);
        }
        public string TableName(string tableName) {
            return DoubleQuote(tableName);
        }
        public string ColumnName(string columnName) {
            return DoubleQuote(columnName);
        }
        public string PropertyToTableName(string className,
                                          string propertyName) {
            return DoubleQuote(propertyName);
        }
        public string LogicalColumnName(string columnName,
                                        string propertyName) {
            return String.IsNullOrWhiteSpace(columnName) ?
                DoubleQuote(propertyName) :
                DoubleQuote(columnName);
        }
        private static string DoubleQuote(string raw) {
            // In some cases the identifier is single-quoted.
            // We simply remove the single quotes:
            raw = raw.Replace("`", "");
            return String.Format("\"{0}\"", raw);
        }
    }

}
