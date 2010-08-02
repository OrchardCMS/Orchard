using Orchard.Data.Migration;

namespace Orchard.Themes.DataMigrations {
    public class ThemesDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Themes_ThemeSiteSettingsRecord (Id INTEGER not null, CurrentThemeName TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ThemeSiteSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("CurrentThemeName")
                );

            return 1;
        }
    }
}