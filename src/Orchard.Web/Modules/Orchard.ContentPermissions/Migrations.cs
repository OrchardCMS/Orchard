using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentPermissions {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ContentPermissionsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("Enabled")
                .Column<string>("ViewContent", c => c.Unlimited())
                .Column<string>("ViewOwnContent", c => c.Unlimited())
                .Column<string>("PublishContent", c => c.Unlimited())
                .Column<string>("PublishOwnContent", c => c.Unlimited())
                .Column<string>("EditContent", c => c.Unlimited())
                .Column<string>("EditOwnContent", c => c.Unlimited())
                .Column<string>("DeleteContent", c => c.Unlimited())
                .Column<string>("DeleteOwnContent", c => c.Unlimited())
                );

            ContentDefinitionManager.AlterPartDefinition("ContentPermissionsPart", p => p.Attachable());

            return 1;
        }
    }
}
