using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Migrations
{
    [OrchardFeature("IDeliverable.Widgets.OutputCache")]
    public class OutputCacheMigrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("OutputCachePart", part => part
                .WithDescription("Enables output caching on a per-content item level. Note: currently only supported by widgets.")
                .Attachable());

            return 1;
        }
    }
}