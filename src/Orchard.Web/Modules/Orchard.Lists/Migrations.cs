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
                    .WithPart("AutoroutePart")
                    .WithPart("ContainerPart")
                    .WithPart("MenuPart")
                    .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2"))
                    .Creatable());

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

        public int UpdateFrom3() {

            // TODO: (PH:Autoroute) Copy paths, routes, etc.

            ContentDefinitionManager.AlterTypeDefinition("List",
                cfg => cfg
                    .RemovePart("RoutePart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart"));

            return 4;
        }

    }
}
