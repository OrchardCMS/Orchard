using System;
using System.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Contrib.Cache.Database {
    [OrchardFeature("Orchard.OutputCache.Database")]
    public class DatabaseOutputCacheMigrations : DataMigrationImpl {

        public int Create() {
            // Creating table CacheItemRecord
            SchemaBuilder.CreateTable("CacheItemRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<DateTime>("CachedOnUtc")
                .Column<int>("Duration")
                .Column<int>("GraceTime", c => c.Nullable())
                .Column<DateTime>("ValidUntilUtc")
                .Column<DateTime>("StoredUntilUtc")
                .Column<string>("Output", column => column.Unlimited())
                .Column<string>("ContentType")
                .Column<string>("QueryString", column => column.WithLength(2048))
                .Column<string>("CacheKey", column => column.WithLength(2048))
                .Column<string>("InvariantCacheKey", column => column.WithLength(2048))
                .Column<string>("Url", column => column.WithLength(2048))
                .Column<string>("Tenant")
                .Column<int>("StatusCode")
                .Column<string>("Tags", column => column.Unlimited())
            );

            SchemaBuilder.AlterTable("CacheItemRecord", table => table
                .CreateIndex("IDX_CacheItemRecord_CacheKey", "CacheKey")
            );

            return 2;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("CacheItemRecord",
                    table => {
                        table.DropColumn("ValidFor");
                        table.AddColumn<int>("Duration");
                        table.AddColumn<int>("GraceTime", c => c.Nullable());
                        table.AddColumn<DateTime>("StoredUntilUtc");
                    });

            return 2;
        }
    }
}