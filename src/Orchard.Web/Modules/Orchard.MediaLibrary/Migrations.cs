using Orchard.ContentManagement.MetaData;
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

    }
}