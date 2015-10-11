using System;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;

namespace Orchard.Tasks.Locking.Services {
    public class DistributedLockSchemaBuilder {
        private readonly ShellSettings _shellSettings;
        private readonly SchemaBuilder _schemaBuilder;
        private const string TableName = "Orchard_Framework_DistributedLockRecord";

        public DistributedLockSchemaBuilder(ShellSettings shellSettings, SchemaBuilder schemaBuilder) {
            _shellSettings = shellSettings;
            _schemaBuilder = schemaBuilder;
        }

        public bool EnsureSchema() {
            if (SchemaExists())
                return false;

            CreateSchema();
            return true;
        }

        public void CreateSchema() {
            _schemaBuilder.CreateTable(TableName, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.NotNull().WithLength(512).Unique())
                .Column<string>("MachineName", column => column.WithLength(256))
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("ValidUntilUtc", column => column.Nullable()));

            _schemaBuilder.AlterTable(TableName, table => {
                table.CreateIndex("IDX_DistributedLockRecord_Name", "Name");
            });
        }

        public bool SchemaExists() {
            try {
                var tablePrefix = String.IsNullOrEmpty(_shellSettings.DataTablePrefix) ? "" : _shellSettings.DataTablePrefix + "_";
                _schemaBuilder.ExecuteSql(String.Format("select * from {0}{1}", tablePrefix, TableName));
                return true;
            }
            catch {
                return false;
            }
        }
    }
}