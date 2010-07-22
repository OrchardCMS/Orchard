using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.PublishLater.Models;
using Orchard.Data.Migration;

namespace Orchard.Core.PublishLater.DataMigrations {
    public class PublishLaterDataMigration : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(typeof(PublishLaterPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Display", new ContentLocation { Zone = "metadata", Position = "1" }},
                    {"Summary", new ContentLocation { Zone = "metadata", Position = "1" }},
                    {"SummaryAdmin", new ContentLocation { Zone = "metadata", Position = "1" }},
                    {"Editor", new ContentLocation { Zone = "secondary", Position = "1" }}
                }));
            return 1;
        }
    }
}