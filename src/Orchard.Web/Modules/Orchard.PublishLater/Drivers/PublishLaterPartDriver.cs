using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Services;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using Orchard.PublishLater.ViewModels;
using Orchard.Localization;

namespace Orchard.PublishLater.Drivers {
    public class PublishLaterPartDriver : ContentPartDriver<PublishLaterPart> {
        private const string TemplateName = "Parts/PublishLater";
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

        protected override string Prefix {
            get { return "PublishLater"; }
        }

        protected override DriverResult Display(PublishLaterPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_PublishLater_Metadata",
                             () => shapeHelper.Parts_PublishLater_Metadata(ContentPart: part, ScheduledPublishUtc: part.ScheduledPublishUtc.Value)),
                ContentShape("Parts_PublishLater_Metadata_Summary",
                             () => shapeHelper.Parts_PublishLater_Metadata_Summary(ContentPart: part, ScheduledPublishUtc: part.ScheduledPublishUtc.Value)),
                ContentShape("Parts_PublishLater_Metadata_SummaryAdmin",
                             () => shapeHelper.Parts_PublishLater_Metadata_SummaryAdmin(ContentPart: part, ScheduledPublishUtc: part.ScheduledPublishUtc.Value))
                );
        }

        protected override DriverResult Editor(PublishLaterPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part);
            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
        protected override DriverResult Editor(PublishLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new PublishLaterViewModel(part);
            updater.TryUpdateModel(model, Prefix, null, null);
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
            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        private static PublishLaterViewModel BuildEditorViewModel(PublishLaterPart part) {
            return new PublishLaterViewModel(part);
        }
    }
}