using Orchard.Data.Migration;

namespace IDeliverable.ThemeSettings
{
    public class ThemesDataMigration : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("ThemeProfileRecord", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("Name", c => c.WithLength(128))
                .Column<string>("Description", c => c.Unlimited())
                .Column<string>("Theme")
                .Column<string>("Settings", c => c.Unlimited())
                .Column<bool>("IsCurrent"));

            return 1;
        }
    }
}