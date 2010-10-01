using System.Collections.Generic;
using Orchard.ArchiveLater.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.ArchiveLater {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(typeof(ArchiveLaterPart).Name, cfg => cfg
                .WithLocation(new Dictionary<string, ContentLocation> {
                    {"Default", new ContentLocation { Zone = "metadata", Position = "2" }},
                    {"Editor", new ContentLocation { Zone = "secondary", Position = "2" }}
                }));
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("ArchiveLaterPart", builder => builder.Attachable());
            return 2;
        }
    }
}