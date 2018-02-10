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

            SchemaBuilder.AlterTable("MediaPartRecord", t => t
                .CreateIndex("IDX_MediaPartRecord_FolderPath", "FolderPath")
            );

            ContentDefinitionManager.AlterPartDefinition("MediaPart", part => part
                .Attachable()
                .WithDescription("Turns a content type into a Media. Note: you need to set the stereotype to \"Media\" as well.")
            );

            ContentDefinitionManager.AlterPartDefinition("ImagePart", part => part
                .Attachable()
                .WithDescription("Provides common metadata for an Image Media.")
            );

            ContentDefinitionManager.AlterPartDefinition("VectorImagePart", part => part
                .Attachable()
                .WithDescription("Provides common metadata for a Vector Image Media.")
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

            ContentDefinitionManager.AlterTypeDefinition("Image", td => td
                .DisplayedAs("Image")
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
                .AsImage()
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("VectorImage", td => td
                .DisplayedAs("Vector Image")
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
                .AsVectorImage()
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("Video", td => td
                .DisplayedAs("Video")
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
                .AsVideo()
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("Audio", td => td
                .DisplayedAs("Audio")
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
                .AsAudio()
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("Document", td => td
                .DisplayedAs("Document")
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
                .AsDocument()
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("OEmbed", td => td
               .DisplayedAs("External Media")
               .WithSetting("Stereotype", "Media")
               .WithIdentity()
               .WithPart("CommonPart")
               .WithPart("MediaPart")
               .WithPart("OEmbedPart")
               .WithPart("TitlePart")
           );

            return 7;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("Image", td => td
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("Video", td => td
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("Audio", td => td
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("Document", td => td
                .WithIdentity()
            );

            ContentDefinitionManager.AlterTypeDefinition("OEmbed", td => td
                .WithIdentity()
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
        
        public int UpdateFrom4() {

            SchemaBuilder.AlterTable("MediaPartRecord", t => t
                .CreateIndex("IDX_MediaPartRecord_FolderPath", "FolderPath")
            );
            
            return 5;
        }

        public int UpdateFrom5() {
            ContentDefinitionManager.AlterTypeDefinition("Image", td => td
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
            );

            ContentDefinitionManager.AlterTypeDefinition("Video", td => td
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
            );

            ContentDefinitionManager.AlterTypeDefinition("Audio", td => td
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
            );

            ContentDefinitionManager.AlterTypeDefinition("Document", td => td
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
            );

            return 6;
        }

        public int UpdateFrom6() {
            ContentDefinitionManager.AlterPartDefinition("VectorImagePart", part => part
                .Attachable()
                .WithDescription("Provides common metadata for a Vector Image Media.")
            );

            ContentDefinitionManager.AlterTypeDefinition("VectorImage", td => td
                .DisplayedAs("Vector Image")
                .WithSetting("Stereotype", "Media")
                .WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", "True")
                .WithIdentity()
                .WithPart("CommonPart")
                .WithPart("MediaPart")
                .WithPart("VectorImagePart")
                .WithPart("TitlePart")
            );

            return 7;
        }
    }
}