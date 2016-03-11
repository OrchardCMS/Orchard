using System;
using System.Globalization;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Indexing {
    public class Migrations : DataMigrationImpl {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create() {
            SchemaBuilder.CreateTable("IndexingTaskRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Action")
                    .Column<DateTime>("CreatedUtc")
                    .Column<int>("ContentItemRecord_id")
                );

            return 4;
        }

        public int UpdateFrom1() {

            foreach (var typeDefinition in _contentDefinitionManager.ListTypeDefinitions()) {
                if (typeDefinition.Settings.ContainsKey("TypeIndexing.Included") && Convert.ToBoolean(typeDefinition.Settings["TypeIndexing.Included"], CultureInfo.InvariantCulture)) {
                    typeDefinition.Settings.Remove("TypeIndexing.Included");
                    typeDefinition.Settings["TypeIndexing.Indexes"] = "Search";
                    _contentDefinitionManager.StoreTypeDefinition(typeDefinition);
                }
            }

            return 4; // Returns 4 instead of 2 due to the modified/deleted migrations in UpdateFrom2-3.
        }

        public int UpdateFrom2() {
            // A table for a custom job implementation was here, but since we use JobsQueue that table is deprecated.

            return 4; // See the comment in UpdateFrom1.
        }

        public int UpdateFrom3() {

            SchemaBuilder.DropTable("IndexTaskBatchRecord");

            return 4;
        }
    }
}