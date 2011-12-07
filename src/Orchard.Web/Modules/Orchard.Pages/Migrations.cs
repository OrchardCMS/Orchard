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
                .WithPart("AutoroutePart")
                .WithPart("BodyPart")
                .Creatable());

            return 3;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg.WithPart("CommonPart", p => p.WithSetting("DateEditorSettings.ShowDateEditor", "true")));
            return 2;
        }

        public int UpdateFrom2() {
            // TODO: (PH:Autoroute) Copy routes/titles
            ContentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg
                .RemovePart("RoutePart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart"));
            return 3;
        }
    }
}
