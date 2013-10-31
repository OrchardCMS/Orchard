using Orchard.Data.Migration;

namespace Orchard.OutputCache {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            
            SchemaBuilder.CreateTable("CacheParameterRecord",
                    table => table
                        .Column<int>("Id", c => c.PrimaryKey().Identity())
                        .Column<int>("Duration")
                        .Column<int>("MaxAge")
                        .Column<string>("RouteKey", c => c.WithLength(255))
                    );

            return 6;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("CacheParameterRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<int>("Duration")
                    .Column<string>("RouteKey", c => c.WithLength(255))
                );

            SchemaBuilder.AlterTable("CacheSettingsPartRecord",
                table => table
                    .AddColumn<string>("IgnoredUrls", c => c.Unlimited())
                );

            SchemaBuilder.AlterTable("CacheSettingsPartRecord",
                table => table
                    .AddColumn<bool>("DebugMode", c => c.WithDefault(false))
                );

            return 2;
        }

        public int UpdateFrom2() {

            SchemaBuilder.AlterTable("CacheSettingsPartRecord",
                table => table
                    .AddColumn<bool>("ApplyCulture", c => c.WithDefault(false))
                );

            return 3;
        }

        public int UpdateFrom3() {

            SchemaBuilder.AlterTable("CacheSettingsPartRecord",
                table => table
                    .AddColumn<int>("DefaultMaxAge")
                );

            SchemaBuilder.AlterTable("CacheParameterRecord",
                    table => table
                        .AddColumn<int>("MaxAge")
                    );

            return 4;
        }
        
        public int UpdateFrom4() {

            SchemaBuilder.AlterTable("CacheSettingsPartRecord",
                table => table
                    .AddColumn<string>("VaryQueryStringParameters", c => c.Unlimited())
                );

            return 5;
        }

        public int UpdateFrom5() {

            SchemaBuilder.AlterTable("CacheSettingsPartRecord",
                table => table
                    .AddColumn<string>("VaryRequestHeaders", c => c.Unlimited())
                );

            return 6;
        }
    }
}