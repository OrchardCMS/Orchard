using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Localization.Models;
using Orchard.Data.Migration;

namespace Orchard.Core.Localization.DataMigrations {
    public class LocalizationDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Localization_LocalizedRecord (Id INTEGER not null, CultureId INTEGER, MasterContentItemId INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("LocalizationPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("CultureId")
                .Column<int>("MasterContentItemId")
                );

            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(typeof(LocalizationPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Default", new ContentLocation { Zone = "Primary", Position = "5" }},
                    {"Editor", new ContentLocation { Zone = "Primary", Position = "1" }},
                }));
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("LocalizationPart", builder => builder.Attachable());
            return 3;
        }
    }
}