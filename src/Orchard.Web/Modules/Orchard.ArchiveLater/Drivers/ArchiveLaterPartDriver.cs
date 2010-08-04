using System;
using ArchiveLater.Models;
using ArchiveLater.Services;
using ArchiveLater.ViewModels;
using Orchard;
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

        protected override DriverResult Display(ArchiveLaterPart part, string displayType) {
            var model = new ArchiveLaterViewModel(part) {
                ScheduledArchiveUtc = part.ScheduledArchiveUtc.Value
            };
            return ContentPartTemplate(model, "Parts/ArchiveLater.Metadata").LongestMatch(displayType, "Summary", "SummaryAdmin").Location(part.GetLocation(displayType));
        }

        protected override DriverResult Editor(ArchiveLaterPart part) {
            return ArchiveEditor(part, null);
        }

        protected override DriverResult Editor(ArchiveLaterPart instance, IUpdateModel updater) {
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