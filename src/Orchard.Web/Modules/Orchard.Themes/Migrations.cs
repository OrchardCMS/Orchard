using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Themes {
    public class ThemesDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ThemeSiteSettingsPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("CurrentThemeName")
                );

            return 1;
        }
    }
}