using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
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

            SchemaBuilder.CreateTable("RecentBlogPostsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("BlogId")
                    .Column<int>("Count")
                );

            SchemaBuilder.CreateTable("BlogArchivesPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("BlogId")
                );

            ContentDefinitionManager.AlterPartDefinition("BlogPart", builder => builder
                .WithDescription("Turns content types into a Blog."));

            ContentDefinitionManager.AlterTypeDefinition("Blog",
                cfg => cfg
                    .WithPart("BlogPart")
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-blog\"}]"))
                    .WithPart("MenuPart")
                    .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2"))
                );

            ContentDefinitionManager.AlterPartDefinition("BlogPostPart", builder => builder
                .WithDescription("Turns content types into a BlogPost."));

            ContentDefinitionManager.AlterTypeDefinition("BlogPost",
                cfg => cfg
                    .WithPart("BlogPostPart")
                    .WithPart("CommonPart", p => p
                        .WithSetting("DateEditorSettings.ShowDateEditor", "True"))
                    .WithPart("PublishLaterPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Blog and Title\",\"Pattern\":\"{Content.Container.Path}/{Content.Slug}\",\"Description\":\"my-blog/my-post\"}]"))
                    .WithPart("BodyPart")
                    .Draftable()
                );

            ContentDefinitionManager.AlterPartDefinition("RecentBlogPostsPart", part => part
                .WithDescription("Renders a list of recent blog posts."));

            ContentDefinitionManager.AlterTypeDefinition("RecentBlogPosts",
                cfg => cfg
                    .WithPart("RecentBlogPostsPart")
                    .AsWidgetWithIdentity()
                );

            ContentDefinitionManager.AlterPartDefinition("BlogArchivesPart", part => part
                .WithDescription("Renders an archive of posts for a blog."));

            ContentDefinitionManager.AlterTypeDefinition("BlogArchives",
                cfg => cfg
                    .WithPart("BlogArchivesPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                    .WithIdentity()
                );

            return 7;
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

        public int UpdateFrom4() {
            // adding the new fields required as Routable was removed
            // the user still needs to execute the corresponding migration
            // steps from the migration module

            SchemaBuilder.AlterTable("RecentBlogPostsPartRecord", table => table
                   .AddColumn<int>("BlogId"));

            SchemaBuilder.AlterTable("BlogArchivesPartRecord", table => table
                    .AddColumn<int>("BlogId"));

            return 5;
        }

        public int UpdateFrom5() {
            ContentDefinitionManager.AlterPartDefinition("BlogPart", builder => builder
                .WithDescription("Turns a content type into a Blog."));

            ContentDefinitionManager.AlterPartDefinition("BlogPostPart", builder => builder
                .WithDescription("Turns a content type into a BlogPost."));

            ContentDefinitionManager.AlterPartDefinition("RecentBlogPostsPart", part => part
                .WithDescription("Renders a list of recent blog posts."));

            ContentDefinitionManager.AlterPartDefinition("BlogArchivesPart", part => part
                .WithDescription("Renders an archive of posts for a blog."));

            return 6;
        }

        public int UpdateFrom6() {
            ContentDefinitionManager.AlterTypeDefinition("RecentBlogPosts",
                cfg => cfg
                    .WithIdentity()
                );

            ContentDefinitionManager.AlterTypeDefinition("BlogArchives",
                cfg => cfg
                    .WithIdentity()
                );

           return 7;
       }
    }
}