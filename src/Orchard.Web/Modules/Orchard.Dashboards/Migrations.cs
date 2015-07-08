using Orchard.ContentManagement.MetaData;
using Orchard.Dashboards.Services;
using Orchard.Data.Migration;

namespace Orchard.Dashboards {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("Dashboard", type => type
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("TitlePart")
                .WithPart("LayoutPart", p => p
                    .WithSetting("LayoutTypePartSettings.DefaultLayoutData", DefaultDashboardSelector.DefaultLayout)));

            return 1;
        } 
    }
}