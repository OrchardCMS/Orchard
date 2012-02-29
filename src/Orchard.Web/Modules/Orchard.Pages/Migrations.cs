using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Pages {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("Page", 
                cfg => cfg
                .WithPart("CommonPart", p => p
                    .WithSetting("DateEditorSettings.ShowDateEditor", "true"))
                .WithPart("PublishLaterPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-page'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .WithPart("BodyPart")
                .Creatable());

            return 3;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg.WithPart("CommonPart", p => p.WithSetting("DateEditorSettings.ShowDateEditor", "true")));
            return 2;
        }
    }
}
