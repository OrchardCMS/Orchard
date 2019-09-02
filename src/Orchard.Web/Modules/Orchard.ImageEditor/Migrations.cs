using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.ImageEditor {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("ImageEditor", t => t.WithPart("ImageEditorPart"));
            return 1;
        }
    }
}