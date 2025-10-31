using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.Templates
{
    [OrchardFeature("Orchard.Templates.Razor")]
    public class RazorMigrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterTypeDefinition("Template", type => type
                .WithPart("ShapePart", p => p
                    .WithSetting("ShapePartSettings.Processor", "Razor")));
            return 1;
        }
    }
}