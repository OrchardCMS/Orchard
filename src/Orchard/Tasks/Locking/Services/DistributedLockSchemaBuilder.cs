using System;
using Orchard.Data.Migration.Schema;

namespace Orchard.Tasks.Locking.Services {
    public class DistributedLockSchemaBuilder {
        private readonly SchemaBuilder _schemaBuilder;
        private const string TableName = "Orchard_Framework_DistributedLockRecord";

        public DistributedLockSchemaBuilder(SchemaBuilder schemaBuilder) {
            _schemaBuilder = schemaBuilder;
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
                _schemaBuilder.ExecuteSql($"select 1 from {_schemaBuilder.TableDbName(TableName)}");

                return true;
            }
            catch {
                return false;
            }
        }
    }
}