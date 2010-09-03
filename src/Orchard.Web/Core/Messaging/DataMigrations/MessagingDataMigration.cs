using Orchard.Data.Migration;

namespace Orchard.Core.Messaging.DataMigrations {
    public class MessagingDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("MessageSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("DefaultChannelService")
                );
            return 1;
        }
    }
}