using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.PublishLater {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("PublishLaterPart", builder => builder
                .Attachable()
                .WithDescription("Adds the ability to delay the publication of a content item to a later date and time."));

            return 2;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("PublishLaterPart", builder => builder
                .WithDescription("Adds the ability to delay the publication of a content item to a later date and time."));

            return 2;
        }
    }
}