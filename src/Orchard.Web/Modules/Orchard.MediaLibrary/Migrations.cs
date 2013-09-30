using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.MediaLibrary {
    public class MediaDataMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("MediaPartRecord", t => t
                .ContentPartRecord()
                .Column<string>("MimeType")
                .Column<string>("Caption", c => c.Unlimited())
                .Column<string>("AlternateText", c => c.Unlimited())
                .Column<string>("FolderPath", c => c.WithLength(2048))
                .Column<string>("FileName", c => c.WithLength(2048))
                );

            ContentDefinitionManager.AlterTypeDefinition("Image", td => td
                .DisplayedAs("Image")
                .WithSetting("Stereotype", "Media")
                .WithPart("CommonPart")
                .WithPart("MediaPart")
                .WithPart("ImagePart")
                .WithPart("TitlePart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Video", td => td
                .DisplayedAs("Video")
                .WithSetting("Stereotype", "Media")
                .WithPart("CommonPart")
                .WithPart("MediaPart")
                .WithPart("VideoPart")
                .WithPart("TitlePart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Audio", td => td
                .DisplayedAs("Audio")
                .WithSetting("Stereotype", "Media")
                .WithPart("CommonPart")
                .WithPart("MediaPart")
                .WithPart("AudioPart")
                .WithPart("TitlePart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Document", td => td
                .DisplayedAs("Document")
                .WithSetting("Stereotype", "Media")
                .WithPart("CommonPart")
                .WithPart("MediaPart")
                .WithPart("DocumentPart")
                .WithPart("TitlePart")
            );

            ContentDefinitionManager.AlterTypeDefinition("OEmbed", td => td
               .DisplayedAs("External Media")
               .WithSetting("Stereotype", "Media")
               .WithPart("CommonPart")
               .WithPart("MediaPart")
               .WithPart("OEmbedPart")
               .WithPart("TitlePart")
           );

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("Image", td => td
                .WithPart("IdentityPart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Video", td => td
                .WithPart("IdentityPart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Audio", td => td
                .WithPart("IdentityPart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Document", td => td
                .WithPart("IdentityPart")
            );

            ContentDefinitionManager.AlterTypeDefinition("OEmbed", td => td
                .WithPart("IdentityPart")
            );

            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition("MediaPart", part => part
                .Attachable()
                .WithDescription("Turns a content type into a Media. Note: you need to set the stereotype to \"Media\" as well.")
            );

            ContentDefinitionManager.AlterPartDefinition("ImagePart", part => part
                            .Attachable()
                            .WithDescription("Provides common metadata for an Image Media.")
                        );

            ContentDefinitionManager.AlterPartDefinition("VideoPart", part => part
                            .Attachable()
                            .WithDescription("Provides common metadata for a Video Media.")
                        );

            ContentDefinitionManager.AlterPartDefinition("AudioPart", part => part
                            .Attachable()
                            .WithDescription("Provides common metadata for an Audio Media.")
                        );

            ContentDefinitionManager.AlterPartDefinition("DocumentPart", part => part
                            .Attachable()
                            .WithDescription("Provides common metadata for a Document Media.")
                        );

            ContentDefinitionManager.AlterPartDefinition("OEmbedPart", part => part
                            .Attachable()
                            .WithDescription("Provides common metadata for an OEmbed Media.")
                        );

            return 4;
        }
    }
}