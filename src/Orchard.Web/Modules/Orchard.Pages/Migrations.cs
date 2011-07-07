using Orchard.ContentManagement.Definition;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Pages {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("Page", 
                cfg => cfg
                .WithPart("CommonPart", p => p
                    .WithSetting("CommonTypePartSettings.ShowCreatedUtcEditor", "true"))
                .WithPart("PublishLaterPart")
                .WithPart("RoutePart")
                .WithPart("BodyPart")
                .Creatable());

            return 1;
        }
    }
}
