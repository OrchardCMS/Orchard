using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.DynamicForms {
    public class Migrations : DataMigrationImpl {
        private const string DefaultFormLayoutData =
@"{
    ""elements"": [
        {
            ""typeName"": ""Orchard.Layouts.Elements.Canvas"",
            ""elements"": [
                {
                    ""typeName"": ""Orchard.DynamicForms.Elements.Form"",
                    ""data"": ""FormName=Untitled&amp;FormAction=&amp;FormMethod=POST&amp;FormBindingContentType=&amp;Publication=Draft&amp;Notification=&amp;RedirectUrl="",
                    ""elements"": [
                        {
                            ""typeName"": ""Orchard.DynamicForms.Elements.Button"",
                            ""data"": ""InputName=&amp;FormBindingContentType=&amp;Text=Submit""
                        }
                    ]
                }
            ]
        }
    ]
}";

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
                 .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-form\"}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .WithPart("LayoutPart", p => p
                    .WithSetting("LayoutTypePartSettings.DefaultLayoutData", DefaultFormLayoutData))
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
                    .WithSetting("LayoutTypePartSettings.DefaultLayoutData", DefaultFormLayoutData))
                .WithSetting("Stereotype", "Widget")
                .DisplayedAs("Form Widget"));
            return 1;
        }
    }
}