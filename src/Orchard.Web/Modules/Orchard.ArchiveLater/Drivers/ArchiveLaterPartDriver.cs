using System;
using Orchard.ArchiveLater.Models;
using Orchard.ArchiveLater.Services;
using Orchard.ArchiveLater.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Orchard.ArchiveLater.Drivers {
    public class ArchiveLaterPartDriver : ContentPartDriver<ArchiveLaterPart> {
        private const string TemplateName = "Parts/ArchiveLater";
        private readonly IArchiveLaterService _archiveLaterService;

        public ArchiveLaterPartDriver(
            IOrchardServices services,
            IArchiveLaterService archiveLaterService) {
            _archiveLaterService = archiveLaterService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix { get { return "ArchiveLater"; } }

        protected override DriverResult Display(ArchiveLaterPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_ArchiveLater_Metadata_SummaryAdmin",
                                shape => shape
                                             .ContentPart(part)
                                             .ScheduledArchiveUtc(part.ScheduledArchiveUtc.Value)
                                             .IsPublished(part.ContentItem.VersionRecord != null && part.ContentItem.VersionRecord.Published)
                                             );
        }

        protected override DriverResult Editor(ArchiveLaterPart part, dynamic shapeHelper) {
            var model = new ArchiveLaterViewModel(part);

            model.ScheduledArchiveUtc = part.ScheduledArchiveUtc.Value;
            model.ArchiveLater = model.ScheduledArchiveUtc.HasValue;
            model.ScheduledArchiveDate = model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value.ToLocalTime().ToShortDateString() : String.Empty;
            model.ScheduledArchiveTime = model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value.ToLocalTime().ToShortTimeString() : String.Empty;

            return ContentShape("Parts_ArchiveLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(ArchiveLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ArchiveLaterViewModel(part);

            if (updater.TryUpdateModel(model, Prefix, null, null) ) {
                if ( model.ArchiveLater ) {
                    DateTime scheduled;
                    if ( DateTime.TryParse(string.Format("{0} {1}", model.ScheduledArchiveDate, model.ScheduledArchiveTime), out scheduled) )
                        model.ScheduledArchiveUtc = scheduled.ToUniversalTime();
                    _archiveLaterService.ArchiveLater(model.ContentItem, model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value : DateTime.MaxValue);
                }
                else {
                    _archiveLaterService.RemoveArchiveLaterTasks(model.ContentItem);
                }
            }

            return ContentShape("Parts_ArchiveLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
    }
}
