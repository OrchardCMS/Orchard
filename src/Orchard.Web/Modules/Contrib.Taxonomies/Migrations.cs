using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Contrib.Taxonomies {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TaxonomyPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("TermTypeName", column => column.WithLength(255))
                );

            SchemaBuilder.CreateTable("TermPartRecord", 
                table => table
                    .ContentPartRecord()
                    .Column<string>("Path", column => column.WithLength(255))
                    .Column<int>("TaxonomyId")
                    .Column<int>("Count")
                    .Column<int>("Weight")
                    .Column<bool>("Selectable")
                );

            SchemaBuilder.CreateTable("TermContentItem", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Field", column => column.WithLength(50))
                    .Column<int>("TermRecord_id")
                    .Column<int>("TermsPartRecord_id")
                );

            SchemaBuilder.CreateTable("TaxonomyMenuPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("TaxonomyPartRecord_id")
                    .Column<int>("TermPartRecord_id")
                    .Column<bool>("DisplayTopMenuItem")
                    .Column<int>("LevelsToDisplay")
                    .Column<bool>("DisplayContentCount")
                    .Column<bool>("HideEmptyTerms")
                );

            ContentDefinitionManager.AlterTypeDefinition("Taxonomy",
                 cfg => cfg
                     .WithPart("TaxonomyPart")
                     .WithPart("CommonPart")
                     .WithPart("TitlePart")
                     .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-taxonomy'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                );

            ContentDefinitionManager.AlterTypeDefinition("TaxonomyMenu",
                cfg => cfg
                    .WithPart("TaxonomyMenuPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            SchemaBuilder.CreateTable("TaxonomyMenuItemPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("RenderMenuItem")
                    .Column<string>("Position", c => c.WithLength(30))
                    .Column<string>("Name", c => c.WithLength(255))
                );

            SchemaBuilder.CreateTable("TermsPartRecord",
                table => table
                    .ContentPartRecord()
                );

            SchemaBuilder.CreateTable("TermWidgetPartRecord",
                            table => table
                                .ContentPartRecord()
                                .Column<int>("TaxonomyPartRecord_id")
                                .Column<int>("TermPartRecord_id")
                                .Column<int>("Count")
                                .Column<string>("OrderBy")
                                .Column<string>("FieldName")
                                .Column<string>("ContentType", c => c.Nullable())
                            );

            ContentDefinitionManager.AlterTypeDefinition("TermWidget",
                cfg => cfg
                    .WithPart("TermWidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 10;
        }
    }
}