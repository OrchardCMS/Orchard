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
            return ContentShape("Parts_ArchiveLater_Metadata_SummaryAdmin",
                                shape => {
                                    part.ScheduledArchiveUtc.Value = DateTime.UtcNow.AddDays(5);

                                    return shape
                                        .ContentPart(part)
                                        .ScheduledArchiveUtc(part.ScheduledArchiveUtc.Value)
                                        .IsPublished(part.ContentItem.VersionRecord != null && part.ContentItem.VersionRecord.Published);
                                });
        }

        protected override DriverResult Editor(ArchiveLaterPart part, dynamic shapeHelper) {
            return ArchiveEditor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ArchiveLaterPart instance, IUpdateModel updater, dynamic shapeHelper) {
            return ArchiveEditor(instance, updater, shapeHelper);
        }

        DriverResult ArchiveEditor(ArchiveLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ArchiveLaterViewModel(part);

            if ( updater != null && updater.TryUpdateModel(model, TemplatePrefix, null, null) ) {
                if (model.ArchiveLater) {
                    DateTime scheduled;
                    if (DateTime.TryParse(string.Format("{0} {1}", model.ScheduledArchiveDate, model.ScheduledArchiveTime), out scheduled))
                        model.ScheduledArchiveUtc = scheduled.ToUniversalTime();
                    _archiveLaterService.ArchiveLater(model.ContentItem, model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value : DateTime.MaxValue);
                }
                else {
                    _archiveLaterService.RemoveArchiveLaterTasks(model.ContentItem);
                }
            }

            return ContentShape("Parts_ArchiveLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/ArchiveLater", Model: model, Prefix: Prefix));
        }
    }
}
