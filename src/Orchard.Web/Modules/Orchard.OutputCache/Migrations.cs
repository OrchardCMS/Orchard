using Orchard.Data.Migration;

namespace Orchard.OutputCache {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            
            SchemaBuilder.CreateTable("CacheParameterRecord",
                    table => table
                        .Column<int>("Id", c => c.PrimaryKey().Identity())
                        .Column<int>("Duration")
                        .Column<int>("GraceTime")
                        .Column<string>("RouteKey", c => c.WithLength(255))
                    );

            return 7;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("CacheParameterRecord",
                table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<int>("Duration")
                    .Column<string>("RouteKey", c => c.WithLength(255))
                );

            return 2;
        }

        public int UpdateFrom2() {
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("CacheParameterRecord",
                    table => table
                        .AddColumn<int>("MaxAge")
                    );

            return 4;
        }
        
        public int UpdateFrom4() {
            return 5;
        }

        public int UpdateFrom5() {
            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("CacheParameterRecord",
                    table => {
                        table.DropColumn("MaxAge");
                        table.AddColumn<int>("GraceTime");
                    });

            return 7;
        }
    }
}