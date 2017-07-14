using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Glimpse.Extensions;
using Orchard.Glimpse.Services;
using Orchard.Glimpse.Tabs.Parts;

namespace Orchard.Glimpse.Models {
    public class GlimpseDriverResult : DriverResult {
        private readonly IGlimpseService _glimpseService;

        public GlimpseDriverResult(DriverResult originalDriverResult, IGlimpseService glimpseService) {
            _glimpseService = glimpseService;
            OriginalDriverResult = originalDriverResult;

            ContentField = originalDriverResult?.ContentField;
            ContentPart = originalDriverResult?.ContentPart;
        }
        public DriverResult OriginalDriverResult { get; set; }

        public override void Apply(BuildDisplayContext context) {
            _glimpseService.PublishTimedAction(() => OriginalDriverResult.Apply(context), t => new PartMessage {
                ContentId = context.ContentItem.Id,
                ContentName = context.ContentItem.GetContentName(),
                ContentType = context.ContentItem.ContentType,
                DisplayType = context.DisplayType,
                PartDefinition = context.ContentPart?.PartDefinition,
                Duration = t.Duration
            }, TimelineCategories.Parts, "Display Part: " + (ContentPart == null ? context.ContentItem.ContentType : ContentPart.PartDefinition.Name), context.ContentItem.GetContentName());
        }

        public override void Apply(BuildEditorContext context) {
            OriginalDriverResult.Apply(context);
        }
    }
}