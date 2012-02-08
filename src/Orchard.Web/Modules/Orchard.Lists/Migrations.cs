using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Lists {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("List", 
                cfg=>cfg
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .WithPart("ContainerPart")
                    .WithPart("MenuPart")
                    .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2"))
                    .Creatable());

            ContentDefinitionManager.AlterTypeDefinition("List",
                cfg => cfg.WithPart("AutoroutePart", 
                    builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-list'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                ));


            return 4;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("List", cfg => cfg.WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2")));
            return 3;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("List", cfg => cfg.WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2")));
            return 3;
        }
    }
}
