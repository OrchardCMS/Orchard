using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Routable.Models;
using Orchard.Data.Migration;

namespace Orchard.Core.Routable.DataMigrations {
    public class RoutableDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Routable_RoutableRecord (Id INTEGER not null, Title TEXT, Slug TEXT, Path TEXT, ContentItemRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("RoutePartRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("Title", column => column.WithLength(1024))
                .Column<string>("Slug")
                .Column<string>("Path", column => column.WithLength(2048))
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(typeof(RoutePart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Editor", new ContentLocation { Zone = "primary", Position = "before.5" }}
                } ));

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("RoutePart", builder => builder.Attachable());
            return 3;
        }
    }
}