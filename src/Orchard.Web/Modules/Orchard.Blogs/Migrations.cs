using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Blogs {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("BlogPartArchiveRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Year")
                    .Column<int>("Month")
                    .Column<int>("PostCount")
                    .Column<int>("BlogPart_id")
                );

            SchemaBuilder.CreateTable("BlogPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Description", c => c.Unlimited())
                    .Column<int>("PostCount")
                );

            SchemaBuilder.CreateTable("RecentBlogPostsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("BlogSlug")
                    .Column<int>("Count")
                );

            SchemaBuilder.CreateTable("BlogArchivesPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("BlogSlug", c => c.WithLength(255))
                );

            ContentDefinitionManager.AlterTypeDefinition("Blog",
                cfg => cfg
                    .WithPart("BlogPart")
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("MenuPart")
                    .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2"))
                );

            ContentDefinitionManager.AlterTypeDefinition("BlogPost",
                cfg => cfg
                    .WithPart("BlogPostPart")
                    .WithPart("CommonPart", p => p
                        .WithSetting("DateEditorSettings.ShowDateEditor", "true"))
                    .WithPart("PublishLaterPart")
                    .WithPart("RoutePart")
                    .WithPart("BodyPart")
                );
            
            ContentDefinitionManager.AlterTypeDefinition("RecentBlogPosts",
                cfg => cfg
                    .WithPart("RecentBlogPostsPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            ContentDefinitionManager.AlterTypeDefinition("BlogArchives",
                cfg => cfg
                    .WithPart("BlogArchivesPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 4;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Blog", cfg => cfg.WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2")));
            return 3;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition("Blog", cfg => cfg.WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2")));
            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterTypeDefinition("BlogPost", cfg => cfg.WithPart("CommonPart", p => p.WithSetting("DateEditorSettings.ShowDateEditor", "true")));
            return 4;
        }
    }
}