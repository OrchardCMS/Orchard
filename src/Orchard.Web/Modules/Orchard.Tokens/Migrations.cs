using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Tokens {
    public class Migrations : DataMigrationImpl {

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