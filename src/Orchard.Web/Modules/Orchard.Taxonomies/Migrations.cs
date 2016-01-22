using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Taxonomies {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TaxonomyPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TermTypeName", column => column.WithLength(255))
                .Column<bool>("IsInternal")
            );

            SchemaBuilder.CreateTable("TermPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Path", column => column.WithLength(255))
                .Column<int>("TaxonomyId")
                .Column<int>("Count")
                .Column<int>("Weight")
                .Column<bool>("Selectable")
            ).AlterTable("TermPartRecord", table => table
                .CreateIndex("IDX_Path", "Path")
            );

            SchemaBuilder.CreateTable("TermContentItem", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Field", column => column.WithLength(50))
                .Column<int>("TermRecord_id")
                .Column<int>("TermsPartRecord_id")
            );

            ContentDefinitionManager.AlterTypeDefinition("Taxonomy", cfg => cfg
                .WithPart("TaxonomyPart")
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-taxonomy\"}]")
                .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
            );

            SchemaBuilder.CreateTable("TermsPartRecord", table => table
                .ContentPartRecord()
            );

            ContentDefinitionManager.AlterTypeDefinition("TaxonomyNavigationMenuItem",
               cfg => cfg
                   .WithPart("TaxonomyNavigationPart")
                   .WithPart("MenuPart")
                   .WithPart("CommonPart")
                   .DisplayedAs("Taxonomy Link")
                   .WithSetting("Description", "Injects menu items from a Taxonomy")
                   .WithSetting("Stereotype", "MenuItem")
               );

            return 4;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("TaxonomyNavigationMenuItem",
               cfg => cfg
                   .WithPart("TaxonomyNavigationPart")
                   .WithPart("MenuPart")
                   .WithPart("CommonPart")
                   .DisplayedAs("Taxonomy Link")
                   .WithSetting("Description", "Injects menu items from a Taxonomy")
                   .WithSetting("Stereotype", "MenuItem")
               );

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("TermPartRecord", table => table
                .CreateIndex("IDX_Path", "Path")
            );

            return 4;
        }
    }
}