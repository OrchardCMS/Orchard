using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Migrations {
    [OrchardFeature("IDeliverable.Widgets")]
    public class WidgetsContainerMigrations : DataMigrationImpl {
        public int Create() {

            SchemaBuilder.CreateTable("WidgetExPartRecord", table => table
                .ContentPartVersionRecord()
                .Column<int>("HostId"));

            ContentDefinitionManager.AlterPartDefinition("WidgetExPart", part => part.Attachable(false));

            ContentDefinitionManager.AlterPartDefinition("WidgetsContainerPart", part => part
                .Attachable()
                .WithDescription("Enables content items to contain widgets, removing the need to create a layer rule per content item."));

            ContentDefinitionManager.AlterTypeDefinition("WidgetsPage", type => type
                .WithPart("CommonPart", p => p
                    .WithSetting("DateEditorSettings.ShowDateEditor", "true"))
                .WithPart("PublishLaterPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", part => part
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-page'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .WithPart("BodyPart")
                .WithPart("WidgetsContainerPart")
                .Listable()
                .Creatable());

            return 1;
        }
    }
}