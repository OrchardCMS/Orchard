using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentPermissions {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ContentPermissionsPart", p => p
                .Attachable()
                .WithDescription("Provides access control configuration on a per content item level."));

            return 3;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("ContentPermissionsPart", p => p
                .WithDescription("Provides access control configuration on a per content item level."));

            return 2;
        }

        public int UpdateFrom2() {
            
            // auto-upgrade to 3 as UpdateFrom1 is incorrectly returning 2
            return 3;
        }
    }
}
