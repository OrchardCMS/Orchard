using Orchard.Data.Migration;

namespace Orchard.Warmup {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("WarmupSettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Urls", column => column.Unlimited())
                    .Column<bool>("Scheduled")
                    .Column<int>("Delay")
                    .Column<bool>("OnPublish")
                );

            return 1;
        }
    }
}
