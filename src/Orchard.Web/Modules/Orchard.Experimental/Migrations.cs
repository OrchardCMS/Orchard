using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.Experimental {
    [OrchardFeature("Orchard.Experimental.TestingLists")]
    public class TestingListsMigrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("ListItem",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("AutoroutePart")
                    .WithPart("BodyPart")
                    .WithPart("ContainablePart")
                    .Creatable()
                );

            ContentDefinitionManager.AlterTypeDefinition("Page",
                cfg => cfg
                    .WithPart("ContainablePart")
                );

            return 1;
        }
    }
}