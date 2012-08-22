using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.CustomForms {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("CustomForm",
                cfg => cfg
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-form'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                    .WithPart("MenuPart")
                    .WithPart("CustomFormPart")
                    .DisplayedAs("Custom Form")
                    .Draftable()
                );

            SchemaBuilder.CreateTable("CustomFormPartRecord", table => table.ContentPartVersionRecord()
                .Column<string>("ContentType", c => c.WithLength(255))
                .Column<bool>("CustomMessage")
                .Column<string>("Message", c => c.Unlimited())
                .Column<bool>("Redirect")
                .Column<string>("RedirectUrl", c => c.Unlimited())
                .Column<bool>("SaveContentItem")
                );

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("CustomFormWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("CustomFormPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 2;
        }

        public void Uninstall() {
            ContentDefinitionManager.DeleteTypeDefinition("CustomForm");
        }
    }
}