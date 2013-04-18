using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.MediaLibrary.Services;
using Orchard.Taxonomies.Models;

namespace Orchard.MediaLibrary {
    public class MediaDataMigration : DataMigrationImpl {
        private readonly IContentManager _contentManager;

        public MediaDataMigration(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public int Create() {

            SchemaBuilder.CreateTable("MediaPartRecord", t => t
                .ContentPartRecord()
                .Column<string>("MimeType")
                .Column<string>("Caption", c => c.Unlimited())
                .Column<string>("AlternateText", c => c.Unlimited())
                .Column<int>("TermPartRecord_id")
                .Column<string>("Resource", c => c.WithLength(2048))
                );

            var taxonomy = _contentManager.New<TaxonomyPart>("Taxonomy");
            taxonomy.IsInternal = true;
            taxonomy.Name = MediaLibraryService.MediaLocation;

            _contentManager.Create(taxonomy);

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

            return 1;
        }
    }
}