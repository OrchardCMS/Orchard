using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace IDeliverable.Slides
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterPartDefinition("SlideshowPart", part => part
                .Attachable()
                .WithDescription("Turns your content item into a slide show."));

            ContentDefinitionManager.AlterTypeDefinition("Slideshow", type => type
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-slideshow\"}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .WithPart("SlideshowPart", p => p
                    .WithSetting("SlideshowSettings.Engine", "JCarousel"))
                .Draftable()
                .Creatable()
                .Listable());

            ContentDefinitionManager.AlterTypeDefinition("SlideshowWidget", type => type
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("WidgetPart")
                .WithPart("SlideshowPart", p => p
                    .WithSetting("SlideshowSettings.Engine", "JCarousel"))
                .WithSetting("Stereotype", "Widget"));

            return 1;
        }
    }
}