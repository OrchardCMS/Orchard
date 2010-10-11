using System;
using Orchard;
using Orchard.ArchiveLater.Models;
using Orchard.ArchiveLater.Services;
using Orchard.ArchiveLater.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Services;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Localization;

namespace ArchiveLater.Drivers {
    public class ArchiveLaterPartDriver : ContentPartDriver<ArchiveLaterPart> {
        private const string TemplatePrefix = "ArchiveLater";
        private readonly ICommonService _commonService;
        private readonly IArchiveLaterService _archiveLaterService;

        public ArchiveLaterPartDriver(
            IOrchardServices services,
            ICommonService commonService,
            IArchiveLaterService archiveLaterService) {
            _commonService = commonService;
            _archiveLaterService = archiveLaterService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(ArchiveLaterPart part, string displayType, dynamic shapeHelper) {
            var metadata = shapeHelper.Parts_ArchiveLater_Metadata(ContentPart: part, ScheduledArchiveUtc: part.ScheduledArchiveUtc.Value);
            if (!string.IsNullOrWhiteSpace(displayType))
                metadata.Metadata.Type = string.Format("{0}.{1}", metadata.Metadata.Type, displayType);
            var location = part.GetLocation(displayType);
            return ContentShape(metadata).Location(location);
        }

        protected override DriverResult Editor(ArchiveLaterPart part, dynamic shapeHelper) {
            return ArchiveEditor(part, null);
        }

        protected override DriverResult Editor(ArchiveLaterPart instance, IUpdateModel updater, dynamic shapeHelper) {
            return ArchiveEditor(instance, updater);
        }

        DriverResult ArchiveEditor(ArchiveLaterPart part, IUpdateModel updater) {
            var model = new ArchiveLaterViewModel(part);

            if ( updater != null && updater.TryUpdateModel(model, TemplatePrefix, null, null) ) {
                if (model.ArchiveLater) {
                    DateTime scheduled;
                    if (DateTime.TryParse(string.Format("{0} {1}", model.ScheduledArchiveDate, model.ScheduledArchiveTime), out scheduled))
                        model.ScheduledArchiveUtc = scheduled.ToUniversalTime();
                    _archiveLaterService.ArchiveLater(model.ContentItem, model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value : DateTime.MaxValue);
                    //Services.Notifier.Information(T("{0} has been scheduled for publishing!", model.ContentItem.TypeDefinition.DisplayName));
                }
                else {
                    //_archiveLaterService.RemoveArchiveLaterTasks(model.ContentItem);
                }
            }

            return ContentPartTemplate(model, "Parts/ArchiveLater", TemplatePrefix).Location(part.GetLocation("Editor"));
        }
    }
}