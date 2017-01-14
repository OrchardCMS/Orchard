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

        public int UpdateFrom2() {
            //We introduced the concept of localization sets, so we need to create them:
            // - Add the column to the table:
            SchemaBuilder.AlterTable("LocalizationPartRecord",
                table => table
                    .AddColumn<int>("LocalizationSetId")
                );
            // - Initialize the value for existing localization parts:
            //Given a record for a ContentItem with Id CIId, with MasterContentItemId MCId, the LocalizationSetId is:
            // - CIId, if MCId == 0: this item is its own master
            // - The MasterContent's LocalizationSetId in all other cases.
            //Since the MasterContent is the one we translated from (not the one that started the translation chain) we have to check several times
            // CIId | MCId  | LocalizationSetId we want
            // 1    | 0     | 1 : this item is its own master
            // 2    | 1     | 1
            // 3    | 2     | 1 : this item is a translation of 2, that is in turn a translation of 1
            if (_repositoryLocalizationPart.Table.Count() > 0) {
                //TODO: to handle very large dbs, we need to do Skip() Take() both for masters and children. The masters should also be stored
                //      differently for when we use them for the children. Maybe a List<Tuple<int,List<int>>>.
                //First we do the items that are their own masters
                //These ones have MasterContentItemId equals 0 or equals their Id value
                var masters = _repositoryLocalizationPart.Table.Where(lpr => lpr.MasterContentItemId == 0 || lpr.MasterContentItemId == lpr.Id).ToList();
                masters.ForEach(ma => ma.LocalizationSetId = ma.Id);
                //Then we do the rest of the items in each localization set
                //These ones have MasterContentItemId greater than zero and different from their own Id value
                //Unmanaging items having MasterContentItemId equals their own Id value may result in an infinite loop
                var children = _repositoryLocalizationPart.Table.Where(lpr => lpr.MasterContentItemId != 0 && lpr.MasterContentItemId != lpr.Id).ToList();
                var previousChildrenCount = 0;
                while (previousChildrenCount != children.Count) { //exit loop if no children was removed during last iteration (condition to avoid infinite loop)
                    previousChildrenCount = children.Count;
                    if (masters.Count >= children.Count) {
                        masters.ForEach(ma => {
                            children.Where(ch => ch.MasterContentItemId == ma.Id).ToList().ForEach(ch => ch.LocalizationSetId = ma.LocalizationSetId);
                        });
                    }
                    else {
                        children.ForEach(ch => {
                            var master = masters.FirstOrDefault(ma => ch.MasterContentItemId == ma.Id);
                            if (master != null) {
                                ch.LocalizationSetId = master.LocalizationSetId;
                            }
                        });
                    }
                    masters.AddRange(children.Where(ch => ch.LocalizationSetId != 0));
                    children.RemoveAll(lpr => lpr.LocalizationSetId != 0);
                }

                //remaining children are considered as masters
                if(children.Count > 0) {
                    children.ForEach(ch => ch.LocalizationSetId = ch.Id);
                }
            }

            return 3;
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