using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using System.Linq;

namespace Orchard.Localization {
    public class Migrations : DataMigrationImpl {

        private readonly IRepository<LocalizationPartRecord> _repositoryLocalizationPart;

        public Migrations(IRepository<LocalizationPartRecord> repositoryLocalizationPart) {
            _repositoryLocalizationPart = repositoryLocalizationPart;
        }

        public int Create() {
            SchemaBuilder.CreateTable("LocalizationPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("CultureId")
                    .Column<int>("MasterContentItemId")
                );

            ContentDefinitionManager.AlterPartDefinition("LocalizationPart", builder => builder.Attachable());

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("LocalizationPart", builder => builder
                .WithDescription("Provides the user interface to localize content items."));

            return 2;
        }
    }

    [OrchardFeature("Orchard.Localization.Transliteration")]
    public class TransliterationMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TransliterationSpecificationRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("CultureFrom")
                    .Column<string>("CultureTo")
                    .Column<string>("Rules", c => c.Unlimited())
                );

            return 1;
        }
    }
}