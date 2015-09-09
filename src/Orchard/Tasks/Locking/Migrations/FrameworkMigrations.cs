using System;
using Orchard.Data.Migration;

namespace Orchard.Tasks.Locking.Migrations {
    public class FrameworkMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("DistributedLockRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.NotNull().WithLength(512).Unique())
                .Column<string>("MachineName", column => column.WithLength(256))
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("ValidUntilUtc", column => column.Nullable()));

            SchemaBuilder.AlterTable("DistributedLockRecord", table => {
                table.CreateIndex("IDX_DistributedLockRecord_Name", "Name");
            });

            return 1;
        }
    }
}