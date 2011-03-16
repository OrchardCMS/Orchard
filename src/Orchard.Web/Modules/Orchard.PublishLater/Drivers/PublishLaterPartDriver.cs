using System;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Services;
using Orchard.Mvc;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using Orchard.PublishLater.ViewModels;
using Orchard.Localization;
using System.Globalization;

namespace Orchard.PublishLater.Drivers {
    public class PublishLaterPartDriver : ContentPartDriver<PublishLaterPart> {
        private const string TemplateName = "Parts/PublishLater";
        private readonly IPublishLaterService _publishLaterService;
        private const string DatePattern = "M/d/yyyy";
        private const string TimePattern = "h:mm tt";

        public PublishLaterPartDriver(
            IOrchardServices services,
            ICommonService commonService,
            IPublishLaterService publishLaterService) {
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
            // date and time are formatted using the same patterns as DateTimePicker is, preventing other cultures issues
            var model = new PublishLaterViewModel(part) {
                ScheduledPublishUtc = part.ScheduledPublishUtc.Value,
                ScheduledPublishDate = part.ScheduledPublishUtc.Value.HasValue && !part.IsPublished() ? part.ScheduledPublishUtc.Value.Value.ToLocalTime().ToString(DatePattern, CultureInfo.InvariantCulture) : String.Empty,
                ScheduledPublishTime = part.ScheduledPublishUtc.Value.HasValue && !part.IsPublished() ? part.ScheduledPublishUtc.Value.Value.ToLocalTime().ToString(TimePattern, CultureInfo.InvariantCulture) : String.Empty
            };

            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
        protected override DriverResult Editor(PublishLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new PublishLaterViewModel(part);

            updater.TryUpdateModel(model, Prefix, null, null);

            if (Services.WorkContext.Resolve<IHttpContextAccessor>().Current().Request.Form["submit.Save"] == "submit.PublishLater") {
                if (!string.IsNullOrWhiteSpace(model.ScheduledPublishDate) && !string.IsNullOrWhiteSpace(model.ScheduledPublishTime)) {
                    DateTime scheduled;
                    string parseDateTime = String.Concat(model.ScheduledPublishDate, " ", model.ScheduledPublishTime);

                    // use an english culture as it is the one used by jQuery.datepicker by default
                    if (DateTime.TryParse(parseDateTime, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AssumeLocal, out scheduled)) {
                        model.ScheduledPublishUtc = part.ScheduledPublishUtc.Value = scheduled.ToUniversalTime();
                        _publishLaterService.Publish(model.ContentItem, model.ScheduledPublishUtc.Value);
                    }
                    else {
                        updater.AddModelError(Prefix, T("{0} is an invalid date and time", parseDateTime));
                    }
                }
                else if (!string.IsNullOrWhiteSpace(model.ScheduledPublishDate) || !string.IsNullOrWhiteSpace(model.ScheduledPublishTime)) {
                    updater.AddModelError(Prefix, T("Both the date and time need to be specified for when this is to be published. If you don't want to schedule publishing then click Save or Publish Now."));
                }
            }

            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override void Importing(PublishLaterPart part, ImportContentContext context) {
            var scheduledUtc = context.Attribute(part.PartDefinition.Name, "ScheduledPublishUtc");
            if (scheduledUtc != null) {
                part.ScheduledPublishUtc.Value = XmlConvert.ToDateTime(scheduledUtc, XmlDateTimeSerializationMode.Utc);
            }
        }

        protected override void Exporting(PublishLaterPart part, ExportContentContext context) {
            var scheduled = part.ScheduledPublishUtc.Value;
            if (scheduled != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("ScheduledPublishUtc", XmlConvert.ToString(scheduled.Value, XmlDateTimeSerializationMode.Utc));
            }
        }
    }
}