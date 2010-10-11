using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Services;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.PublishLater.Models;
using Orchard.Core.PublishLater.Services;
using Orchard.Core.PublishLater.ViewModels;
using Orchard.Localization;

namespace Orchard.Core.PublishLater.Drivers {
    public class PublishLaterPartDriver : ContentPartDriver<PublishLaterPart> {
        private const string TemplatePrefix = "PublishLater";
        private readonly ICommonService _commonService;
        private readonly IPublishLaterService _publishLaterService;

        public PublishLaterPartDriver(
            IOrchardServices services,
            ICommonService commonService,
            IPublishLaterService publishLaterService) {
            _commonService = commonService;
            _publishLaterService = publishLaterService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(PublishLaterPart part, string displayType, dynamic shapeHelper) {
            var metadata = shapeHelper.PublishLater_Metadata(ContentPart: part, ScheduledPublishUtc: part.ScheduledPublishUtc.Value);
            if (!string.IsNullOrWhiteSpace(displayType))
                metadata.Metadata.Type = string.Format("{0}.{1}", metadata.Metadata.Type, displayType);
            var location = part.GetLocation(displayType);
            return ContentShape(metadata).Location(location);
        }

        protected override DriverResult Editor(PublishLaterPart part, dynamic shapeHelper) {
            return PublishEditor(part, null);
        }

        protected override DriverResult Editor(PublishLaterPart instance, IUpdateModel updater, dynamic shapeHelper) {
            return PublishEditor(instance, updater);
        }

        DriverResult PublishEditor(PublishLaterPart part, IUpdateModel updater) {
            var model = new PublishLaterViewModel(part);

            if (updater != null) {
                updater.TryUpdateModel(model, TemplatePrefix, null, null);
                switch (model.Command) {
                    case "PublishNow":
                        _commonService.Publish(model.ContentItem);
                        //Services.Notifier.Information(T("{0} has been published!", model.ContentItem.TypeDefinition.DisplayName));
                        break;
                    case "PublishLater":
                        DateTime scheduled;
                        if (DateTime.TryParse(string.Format("{0} {1}", model.ScheduledPublishUtcDate, model.ScheduledPublishUtcTime), out scheduled))
                            model.ScheduledPublishUtc = scheduled;
                        _publishLaterService.Publish(model.ContentItem, model.ScheduledPublishUtc.HasValue ? model.ScheduledPublishUtc.Value : DateTime.MaxValue);
                        //Services.Notifier.Information(T("{0} has been scheduled for publishing!", model.ContentItem.TypeDefinition.DisplayName));
                        break;
                    case "SaveDraft":
                        //Services.Notifier.Information(T("{0} draft has been saved!", model.ContentItem.TypeDefinition.DisplayName));
                        break;
                }
            }

            return ContentPartTemplate(model, "Parts/PublishLater", TemplatePrefix).Location(part.GetLocation("Editor"));
        }
    }
}