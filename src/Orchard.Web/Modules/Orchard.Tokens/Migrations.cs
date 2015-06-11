using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.Tokens {
    [OrchardFeature("Orchard.Tokens.Feeds")]
    public class FeedsMigrations : DataMigrationImpl {

        public int Create() {

            ContentDefinitionManager.AlterPartDefinition("RssPart",
                cfg => cfg
                    .Attachable()
                    .WithDescription("Attach to a content type to provide custom values in RSS feeds.")
                );

            return 1;
        }
    }
}