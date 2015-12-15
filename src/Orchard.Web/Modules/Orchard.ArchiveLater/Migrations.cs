using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.ArchiveLater {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ArchiveLaterPart", builder => builder
                .Attachable()
                .WithDescription("Adds the ability to delay the unpublishing of a content item to a later date and time."));

            return 2;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("ArchiveLaterPart", builder => builder
                .WithDescription("Adds the ability to delay the unpublising of a content item to a later date and time."));

            return 2;
        }
    }
}
