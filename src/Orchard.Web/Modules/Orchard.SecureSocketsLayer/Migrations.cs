using Orchard.Data.Migration;

namespace Orchard.SecureSocketsLayer {
    public class Migrations : DataMigrationImpl {
        public int Create() {

            SchemaBuilder.CreateTable("SslSettingsPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<bool>("CustomEnabled")
                    .Column<bool>("SecureEverything")
                    .Column<string>("Urls", c => c.Unlimited())
                    .Column<string>("SecureHostName")
                    .Column<string>("InsecureHostName")
                );

            return 1;
        }
    }
}