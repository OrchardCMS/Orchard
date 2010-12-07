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
                                                                    .WithPart("RoutePart")
                                                                    .WithPart("BodyPart")
                                                                    .WithPart("ContainablePart")
                                                                    .Creatable());

            ContentDefinitionManager.AlterTypeDefinition("Page",
                                                         cfg => cfg
                                                                    .WithPart("ContainablePart"));

            //ContentDefinitionManager.AlterTypeDefinition("ListWidget",
            //    cfg => cfg
            //        .WithPart("CommonPart")
            //        .WithPart("WidgetPart")
            //        .WithPart("ListWidgetPart")
            //        .WithSetting("Stereotype", "Widget"));

            return 1;
        }
    }
}