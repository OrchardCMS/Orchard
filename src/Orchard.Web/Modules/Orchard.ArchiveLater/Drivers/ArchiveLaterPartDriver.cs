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

namespace Orchard.ArchiveLater.Drivers {
    public class ArchiveLaterPartDriver : ContentPartDriver<ArchiveLaterPart> {
        private const string TemplateName = "Parts/ArchiveLater";
        private readonly IArchiveLaterService _archiveLaterService;
        private const string DatePattern = "M/d/yyyy";
        private const string TimePattern = "h:mm tt";

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
            var model = new ArchiveLaterViewModel(part) {ScheduledArchiveUtc = part.ScheduledArchiveUtc.Value};

            model.ArchiveLater = model.ScheduledArchiveUtc.HasValue;
            model.ScheduledArchiveDate = model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value.ToLocalTime().ToString(DatePattern, CultureInfo.InvariantCulture) : String.Empty;
            model.ScheduledArchiveTime = model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value.ToLocalTime().ToString(TimePattern, CultureInfo.InvariantCulture) : String.Empty;

            return ContentShape("Parts_ArchiveLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(ArchiveLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ArchiveLaterViewModel(part);

            if (updater.TryUpdateModel(model, Prefix, null, null) ) {
                if ( model.ArchiveLater ) {
                    DateTime scheduled;
                    var parseDateTime = String.Concat(model.ScheduledArchiveDate, " ", model.ScheduledArchiveTime);

                    // use an english culture as it is the one used by jQuery.datepicker by default
                    if (DateTime.TryParse(parseDateTime, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AssumeLocal, out scheduled)) {
                        model.ScheduledArchiveUtc = scheduled.ToUniversalTime();
                        _archiveLaterService.ArchiveLater(model.ContentItem, model.ScheduledArchiveUtc.HasValue ? model.ScheduledArchiveUtc.Value : DateTime.MaxValue);
                    }
                    else {
                        updater.AddModelError(Prefix, T("{0} is an invalid date and time", parseDateTime));
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
