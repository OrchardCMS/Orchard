using Orchard.Data.Migration;

namespace Orchard.Messaging {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("MessageSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("DefaultChannelService", c => c.WithLength(64))
                );

            return 1;
        }
    }
}