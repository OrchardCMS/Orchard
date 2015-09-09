using System;
using System.Linq;
using System.Security.Cryptography;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;

namespace Orchard.DynamicForms {
    public class Migrations : DataMigrationImpl {
        private readonly byte[] _oldLayoutHash = new byte[] { 0x91, 0x10, 0x3b, 0x97, 0xce, 0x1e, 0x1e, 0xc7, 0x7a, 0x41, 0xf7, 0x82, 0xe8, 0x58, 0x85, 0x91 };

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
            return 2;
        }

        public int UpdateFrom1() {
            // if the default layout data was unchanged, fix it with the new default

            var formLayoutPart = ContentDefinitionManager
                .GetTypeDefinition("Form")
                .Parts
                .FirstOrDefault(x => x.PartDefinition.Name == "LayoutPart");

            if (formLayoutPart != null && 
                formLayoutPart.Settings["LayoutTypePartSettings.DefaultLayoutData"] != null) {
                var layout = formLayoutPart.Settings["LayoutTypePartSettings.DefaultLayoutData"];

                if(GetMD5(layout) == _oldLayoutHash) {
                    ContentDefinitionManager.AlterTypeDefinition("Form", type => type
                        .WithPart("LayoutPart", p => p
                            .WithSetting("LayoutTypePartSettings.DefaultLayoutData", DefaultFormLayoutData))
                    );
                }
            }

            var formWidgetLayoutPart = ContentDefinitionManager
                .GetTypeDefinition("FormWidget")
                .Parts
                .FirstOrDefault(x => x.PartDefinition.Name == "LayoutPart");

            if (formWidgetLayoutPart != null &&
                formWidgetLayoutPart.Settings["LayoutTypePartSettings.DefaultLayoutData"] != null) {
                var layout = formWidgetLayoutPart.Settings["LayoutTypePartSettings.DefaultLayoutData"];

                if (GetMD5(layout) == _oldLayoutHash) {
                    ContentDefinitionManager.AlterTypeDefinition("FormWidget", type => type
                        .WithPart("LayoutPart", p => p
                            .WithSetting("LayoutTypePartSettings.DefaultLayoutData", DefaultFormLayoutData))
                    );
                }
            }

            return 2;
        }

        private byte[] GetMD5(string text) {
            byte[] encodedText = System.Text.Encoding.UTF8.GetBytes(text);
            return ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedText);
        }
    }
}