using System;
using Orchard.Data.Migration;

namespace Orchard.Tasks.Locking.Migrations {
    public class FrameworkMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("LockRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.NotNull().WithLength(256))
                .Column<string>("Owner", column => column.WithLength(256))
                .Column<int>("ReferenceCount")
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("ValidUntilUtc"));

            return 1;
        }
    }
}