using System;
using Orchard.Data.Migration;

namespace Orchard.Core.Scheduling.DataMigrations {
    public class SchedulingDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Scheduling_ScheduledTaskRecord (Id  integer, TaskType TEXT, ScheduledUtc DATETIME, ContentItemVersionRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("ScheduledTaskRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("TaskType")
                .Column<DateTime>("ScheduledUtc")
                .Column<int>("ContentItemVersionRecord_id")
                );

            return 0010;
        }
    }
}