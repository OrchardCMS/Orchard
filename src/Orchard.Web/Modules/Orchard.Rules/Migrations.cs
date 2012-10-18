using Orchard.Data.Migration;

namespace Orchard.Rules {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("RuleRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<bool>("Enabled")
                    .Column<string>("Name", c => c.WithLength(1024))
                );

            SchemaBuilder.CreateTable("EventRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<string>("Category", c => c.WithLength(64))
                    .Column<string>("Type", c => c.WithLength(64))
                    .Column<string>("Parameters", c => c.Unlimited())
                    .Column<int>("RuleRecord_id")
                );

            SchemaBuilder.CreateTable("ActionRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<string>("Category", c => c.WithLength(64))
                    .Column<string>("Type", c => c.WithLength(64))
                    .Column<string>("Parameters", c => c.Unlimited())
                    .Column<int>("Position")
                    .Column<int>("RuleRecord_id")
                );

            SchemaBuilder.CreateTable("ScheduledActionTaskRecord",
                table => table
                    .ContentPartVersionRecord()
                );

            SchemaBuilder.CreateTable("ScheduledActionRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<int>("ActionRecord_id")
                    .Column<int>("ScheduledActionTaskRecord_id")
                );

            return 1;
        }
    }
}