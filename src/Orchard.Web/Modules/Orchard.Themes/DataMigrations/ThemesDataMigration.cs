using Orchard.Data.Migration;

namespace Orchard.Themes.DataMigrations {
    public class ThemesDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Themes_ThemeRecord (Id INTEGER not null, ThemeName TEXT, DisplayName TEXT, Description TEXT, Version TEXT, Author TEXT, HomePage TEXT, Tags TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ThemeRecord", table => table
                .ContentPartRecord()
                .Column<string>("ThemeName")
                .Column<string>("DisplayName")
                .Column<string>("Description")
                .Column<string>("Version")
                .Column<string>("Author")
                .Column<string>("HomePage")
                .Column<string>("Tags")
                );

            //CREATE TABLE Orchard_Themes_ThemeSiteSettingsRecord (Id INTEGER not null, CurrentThemeName TEXT, primary key (Id));
            SchemaBuilder.CreateTable("ThemeSiteSettingsRecord", table => table
                .ContentPartRecord()
                .Column<string>("CurrentThemeName")
                );

            return 1;
        }
    }
}