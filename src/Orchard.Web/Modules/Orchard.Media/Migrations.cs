using Orchard.Data.Migration;
using Orchard.Media.Models;

namespace Orchard.Media {
    public class MediaDataMigration : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("MediaSettingsPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("UploadAllowedFileTypeWhitelist", c => c.WithDefault(MediaSettingsPartRecord.DefaultWhitelist).WithLength(255))
                );

            return 1;
        }
    }
}