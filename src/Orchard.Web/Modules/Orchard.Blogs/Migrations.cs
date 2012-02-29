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

            SchemaBuilder.CreateTable("BlogPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Description", c => c.Unlimited())
                    .Column<int>("PostCount")
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

            ContentDefinitionManager.AlterTypeDefinition("Blog",
                cfg => cfg
                    .WithPart("BlogPart")
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-blog'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                    .WithPart("MenuPart")
                    .WithPart("AdminMenuPart", p => p.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", "2"))
                );

            ContentDefinitionManager.AlterTypeDefinition("BlogPost",
                cfg => cfg
                    .WithPart("BlogPostPart")
                    .WithPart("CommonPart", p => p
                        .WithSetting("DateEditorSettings.ShowDateEditor", "true"))
                    .WithPart("PublishLaterPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Blog and Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-blog/my-post'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                    .WithPart("BodyPart")
                    .Draftable()
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

            return 5;
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
    }
}