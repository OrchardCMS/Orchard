using Orchard.Data.Migration;

namespace Orchard.Core.Messaging {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("MessageSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("DefaultChannelService")
                );
            return 1;
        }
    }
}