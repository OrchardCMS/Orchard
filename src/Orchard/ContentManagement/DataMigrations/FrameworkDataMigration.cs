using Orchard.Data.Migration;

namespace Orchard.ContentManagement.DataMigrations {
    public class FrameworkDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ContentItemRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Data", c => c.Unlimited())
                    .Column<int>("ContentType_id")
                );

            SchemaBuilder.CreateTable("ContentItemVersionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Number")
                    .Column<bool>("Published")
                    .Column<bool>("Latest")
                    .Column<string>("Data", c => c.Unlimited())
                    .Column<int>("ContentItemRecord_id", c => c.NotNull())
                );

            SchemaBuilder.CreateTable("ContentTypeRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                );

            SchemaBuilder.CreateTable("CultureRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Culture")
                );

            return 1;
        }

    }
}