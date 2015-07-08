using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.DynamicForms {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("Submission", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<string>("FormName", c => c.WithLength(128))
                .Column<string>("FormData", c => c.Unlimited())
                .Column<DateTime>("CreatedUtc"));

            ContentDefinitionManager.AlterTypeDefinition("Form", type => type
                .WithPart("CommonPart", p => p
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("MenuPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-form\"}]"))
                .WithPart("LayoutPart", p => p
                    .WithSetting("LayoutTypePartSettings.DefaultLayoutData",
                    "{" +
                    "   \"elements\":[" +
                    "   {" +
                    "      \"typeName\":\"Orchard.Layouts.Elements.Canvas\"," +
                    "      \"elements\":[" +
                    "      {" +
                    "         \"typeName\":\"Orchard.DynamicForms.Elements.Form\"," +
                    "         \"data\":\"TypeName=Orchard.DynamicForms.Elements.Form&amp;FormName=Untitled&amp;FormAction=&amp;FormMethod=POST&amp;EnableClientValidation=true&amp;StoreSubmission=true&amp;FormBindingContentType=&amp;Publication=Draft&amp;Notification=&amp;RedirectUrl=\"," +
                    "         \"elements\":[" +
                    "         {" +
                    "            \"typeName\":\"Orchard.DynamicForms.Elements.Button\"," +
                    "            \"data\":\"TypeName=Orchard.DynamicForms.Elements.Button&amp;InputName=&amp;FormBindingContentType=&amp;Text=Submit\"" +
                    "         }]" +
                    "      }]" +
                    "   }]" +
                    "}"))
                .DisplayedAs("Form")
                .Listable()
                .Creatable()
                .Draftable());

            ContentDefinitionManager.AlterTypeDefinition("FormWidget", type => type
                .WithPart("CommonPart", p => p
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false")
                    .WithSetting("DateEditorSettings.ShowDateEditor", "false"))
                .WithPart("WidgetPart")
                .WithPart("LayoutPart", p => p
                    .WithSetting("LayoutTypePartSettings.DefaultLayoutData",
                    "{" +
                        "\"elements\": [{" +
                            "\"typeName\": \"Orchard.DynamicForms.Elements.Form\"," +
                            "\"elements\": [{" +
                                "\"typeName\": \"Orchard.DynamicForms.Elements.Button\"," +
                                "\"state\": \"ButtonText=Submit\"" +
                            "}]" +
                        "}]" +
                    "}"))
                .WithSetting("Stereotype", "Widget")
                .DisplayedAs("Form Widget"));
            return 1;
        }
    }
}