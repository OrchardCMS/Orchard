using System;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Configuration;

namespace Orchard.Tasks.Locking.Services {
    public class DistributedLockSchemaBuilder {
        private readonly SchemaBuilder _schemaBuilder;
        private readonly ShellSettings _shellSettings;
        private const string TableName = "Orchard_Framework_DistributedLockRecord";

        public DistributedLockSchemaBuilder(SchemaBuilder schemaBuilder, ShellSettings shellSettings) {
            _schemaBuilder = schemaBuilder;
            _shellSettings = shellSettings;
        }

        public void CreateSchema() {
            _schemaBuilder.CreateTable(TableName, table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                // Keep 512 chars but avoid inline .Unique() to prevent MySQL TEXT/BLOB key-length errors.
                .Column<string>("Name", column => column.NotNull().WithLength(512))
                .Column<string>("MachineName", column => column.WithLength(256))
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("ValidUntilUtc", column => column.Nullable()));

            // MySQL-specific adjustment to avoid error when adding the unique constraint.
            if (string.Equals(_shellSettings.DataProvider, "MySql", StringComparison.OrdinalIgnoreCase)) {
                // Force VARCHAR(512) on MySQL (some interpreters map >255 to TEXT).
                _schemaBuilder.ExecuteSql(
                    $"ALTER TABLE {_schemaBuilder.TableDbName(TableName)} MODIFY COLUMN Name VARCHAR(512) NOT NULL;",
                    cmd => cmd.ForProvider("MySql"));
            }

            _schemaBuilder.AlterTable(TableName, table => {
                table.AddUniqueConstraint("UQ_DistributedLockRecord_Name", "Name");
                table.CreateIndex("IDX_DistributedLockRecord_Name", "Name");
            });
        }

        public bool SchemaExists() {
            try {
                _schemaBuilder.ExecuteSql($"select 1 from {_schemaBuilder.TableDbName(TableName)}");

                return true;
            }
            catch {
                return false;
            }
        }
    }
}