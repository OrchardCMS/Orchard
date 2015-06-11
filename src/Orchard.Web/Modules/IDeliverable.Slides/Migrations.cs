using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace IDeliverable.Slides {
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {    
            ContentDefinitionManager.AlterPartDefinition("SlideShowPart", part => part
                .Attachable()
                .WithDescription("Turns your content item into a slide show."));

            ContentDefinitionManager.AlterTypeDefinition("SlideShow", type => type
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "True")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-slide-show\"}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .WithPart("SlideShowPart", p => p
                    .WithSetting("SlideShowSettings.Engine", "JCarousel"))
                .Draftable()
                .Creatable()
                .Listable());

            ContentDefinitionManager.AlterTypeDefinition("SlideShowWidget", type => type
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("WidgetPart")
                .WithPart("SlideShowPart", p => p
                    .WithSetting("SlideShowSettings.Engine", "JCarousel"))
                .WithSetting("Stereotype", "Widget"));

            return 1;
        }
    }
}