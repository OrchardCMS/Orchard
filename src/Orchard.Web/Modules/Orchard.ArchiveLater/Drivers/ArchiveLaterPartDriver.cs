using System;
using System.Xml;
using Orchard.ArchiveLater.Models;
using Orchard.ArchiveLater.Services;
using Orchard.ArchiveLater.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System.Globalization;
using Orchard.Localization.Services;

namespace Orchard.ArchiveLater.Drivers {
    public class ArchiveLaterPartDriver : ContentPartDriver<ArchiveLaterPart> {
        private const string TemplateName = "Parts/ArchiveLater";
        private readonly IArchiveLaterService _archiveLaterService;
        private readonly IDateServices _dateServices;

        public ArchiveLaterPartDriver(
            IOrchardServices services,
            IArchiveLaterService archiveLaterService,
            IDateServices dateServices) {
            _archiveLaterService = archiveLaterService;
            _dateServices = dateServices;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T {
            get;
            set;
        }
        public IOrchardServices Services {
            get;
            set;
        }

        protected override string Prefix {
            get {
                return "ArchiveLater";
            }
        }

        protected override DriverResult Display(ArchiveLaterPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_ArchiveLater_Metadata_SummaryAdmin",
                                shape => shape
                                             .ContentPart(part)
                                             .ScheduledArchiveUtc(part.ScheduledArchiveUtc.Value)
                                             .IsPublished(part.ContentItem.VersionRecord != null && part.ContentItem.VersionRecord.Published)
                                             );
        }

        protected override DriverResult Editor(ArchiveLaterPart part, dynamic shapeHelper) {
            var localDate = new Lazy<DateTime>(() => TimeZoneInfo.ConvertTimeFromUtc(part.ScheduledArchiveUtc.Value.Value, Services.WorkContext.CurrentTimeZone));

            var model = new ArchiveLaterViewModel(part) {
                ScheduledArchiveUtc = part.ScheduledArchiveUtc.Value,
                ArchiveLater = part.ScheduledArchiveUtc.Value.HasValue,
                ScheduledArchiveDate = _dateServices.ConvertToLocalDateString(part.ScheduledArchiveUtc.Value, ""),
                ScheduledArchiveTime = _dateServices.ConvertToLocalTimeString(part.ScheduledArchiveUtc.Value, "")
            };

            return ContentShape("Parts_ArchiveLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(ArchiveLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ArchiveLaterViewModel(part);

            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                if (model.ArchiveLater) {
                    try {
                        var utcDateTime = _dateServices.ConvertFromLocalString(model.ScheduledArchiveDate, model.ScheduledArchiveTime);
                        model.ScheduledArchiveUtc = utcDateTime;
                        _archiveLaterService.ArchiveLater(model.ContentItem, model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value : DateTime.MaxValue);
                    }
                    catch (FormatException) {
                        updater.AddModelError(Prefix, T("'{0} {1}' could not be parsed as a valid date and time.", model.ScheduledArchiveDate, model.ScheduledArchiveTime));                        
                    }
                }
                else {
                    _archiveLaterService.RemoveArchiveLaterTasks(model.ContentItem);
                }
            }

            return ContentShape("Parts_ArchiveLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override void Importing(ArchiveLaterPart part, ImportContentContext context) {
            var scheduledUtc = context.Attribute(part.PartDefinition.Name, "ScheduledArchiveUtc");
            if (scheduledUtc != null) {
                part.ScheduledArchiveUtc.Value = XmlConvert.ToDateTime(scheduledUtc, XmlDateTimeSerializationMode.Utc);
            }
        }

        protected override void Exporting(ArchiveLaterPart part, ExportContentContext context) {
            var scheduled = part.ScheduledArchiveUtc.Value;
            if (scheduled != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("ScheduledArchiveUtc", XmlConvert.ToString(scheduled.Value, XmlDateTimeSerializationMode.Utc));
            }
        }
    }
}
