using Orchard.ContentManagement.MetaData;
using System.Data;
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
                .Column<string>("FullWeight", column => column.WithLength(1023))
            ).AlterTable("TermPartRecord", table => {
                table.CreateIndex("IDX_Path", "Path");
                table.CreateIndex("IDX_FullWeight", "FullWeight");
            });

            SchemaBuilder.CreateTable("TermContentItem", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Field", column => column.WithLength(50))
                .Column<int>("TermRecord_id")
                .Column<int>("TermsPartRecord_id")
            ).AlterTable("TermContentItem", table => {
                table.CreateIndex("IDX_TermsPartRecord_id", "TermsPartRecord_id");
                table.CreateIndex("IDX_TermsPartRecord_id_Field", "TermsPartRecord_id", "Field");
            });

            ContentDefinitionManager.AlterTypeDefinition("Taxonomy", cfg => cfg
                .WithPart("TaxonomyPart")
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-taxonomy\"}]"))
            );

            SchemaBuilder.CreateTable("TermsPartRecord", table => table
                .ContentPartRecord()
            );

            ContentDefinitionManager.AlterTypeDefinition("TaxonomyNavigationMenuItem",
               cfg => cfg
                   .WithPart("TaxonomyNavigationPart")
                   .WithPart("MenuPart")
                   .WithPart("CommonPart")
                   .WithIdentity()
                   .DisplayedAs("Taxonomy Link")
                   .WithSetting("Description", "Injects menu items from a Taxonomy")
                   .WithSetting("Stereotype", "MenuItem")
               );

            return 10;
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

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterTypeDefinition("TaxonomyNavigationMenuItem",
               cfg => cfg
                   .WithIdentity()
               );

            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("TermContentItem", table => table
                .CreateIndex("IDX_TermsPartRecord_id", "TermsPartRecord_id")
            );
            return 6;
        }
        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("TermContentItem", table => table
                .CreateIndex("IDX_TermsPartRecord_id_Field", "TermsPartRecord_id", "Field")
            );
            return 7;
        }
        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("TermPartRecord", table => {
                table.AddColumn("FullWeight", DbType.String);
                table.CreateIndex("IDX_FullWeight", "FullWeight");
            });
            return 8;
        }

        // These two updates are done separate here because we cannot alter
        // the FullWeight column as long as there is an index defined over it.
        public int UpdateFrom8() {
            SchemaBuilder.AlterTable("TermPartRecord", table => {
                table.DropIndex("IDX_FullWeight");
            });
            return 9;
        }
        public int UpdateFrom9() {
            SchemaBuilder.AlterTable("TermPartRecord", table => {
                table.AlterColumn("FullWeight", column => {
                    column.WithType(DbType.String);
                    column.WithLength(1023);
                });
                table.CreateIndex("IDX_FullWeight", "FullWeight");
            });
            return 10;
        }
    }
}