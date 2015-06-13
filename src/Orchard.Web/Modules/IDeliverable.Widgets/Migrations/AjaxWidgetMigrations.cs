using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Migrations {
    [OrchardFeature("IDeliverable.Widgets.Ajax")]
    public class AjaxWidgetMigrations : DataMigrationImpl {
        public int Create() {
            
            ContentDefinitionManager.AlterPartDefinition("AjaxifyPart", part => part
                .Attachable()
                .WithDescription("Turns an existing widget type into one that can be loaded asynchronously via AJAX."));

            return 1;
        }
    }
}