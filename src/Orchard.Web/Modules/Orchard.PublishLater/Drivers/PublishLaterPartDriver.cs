using System;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Mvc;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using Orchard.PublishLater.ViewModels;
using Orchard.Localization;
using System.Globalization;
using Orchard.Services;

namespace Orchard.PublishLater.Drivers {
    public class PublishLaterPartDriver : ContentPartDriver<PublishLaterPart> {
        private const string TemplateName = "Parts/PublishLater";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPublishLaterService _publishLaterService;
        private readonly IClock _clock;

        private readonly Lazy<CultureInfo> _cultureInfo;
 
        public PublishLaterPartDriver(
            IOrchardServices services,
            IHttpContextAccessor httpContextAccessor,
            IPublishLaterService publishLaterService,
            IClock clock) {
            _httpContextAccessor = httpContextAccessor;
            _publishLaterService = publishLaterService;
            _clock = clock;
            T = NullLocalizer.Instance;
            Services = services;

            // initializing the culture info lazy initializer
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(Services.WorkContext.CurrentCulture));
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "PublishLater"; }
        }

        protected override DriverResult Display(PublishLaterPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_PublishLater_Metadata",
                             () => shapeHelper.Parts_PublishLater_Metadata(ScheduledPublishUtc: part.ScheduledPublishUtc.Value)),
                ContentShape("Parts_PublishLater_Metadata_Summary",
                             () => shapeHelper.Parts_PublishLater_Metadata_Summary(ScheduledPublishUtc: part.ScheduledPublishUtc.Value)),
                ContentShape("Parts_PublishLater_Metadata_SummaryAdmin",
                             () => shapeHelper.Parts_PublishLater_Metadata_SummaryAdmin(ScheduledPublishUtc: part.ScheduledPublishUtc.Value))
                );
        }

        protected override DriverResult Editor(PublishLaterPart part, dynamic shapeHelper) {
            // date and time are formatted using the same patterns as DateTimePicker is, preventing other cultures issues
            var localDate = new Lazy<DateTime>( () => TimeZoneInfo.ConvertTimeFromUtc(part.ScheduledPublishUtc.Value.Value, Services.WorkContext.CurrentTimeZone));
            var model = new PublishLaterViewModel(part) {
                ScheduledPublishUtc = part.ScheduledPublishUtc.Value,
                ScheduledPublishDate = part.ScheduledPublishUtc.Value.HasValue && !part.IsPublished() ? localDate.Value.ToString("d", _cultureInfo.Value) : String.Empty,
                ScheduledPublishTime = part.ScheduledPublishUtc.Value.HasValue && !part.IsPublished() ? localDate.Value.ToString("t", _cultureInfo.Value) : String.Empty,
            };

            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }
        protected override DriverResult Editor(PublishLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new PublishLaterViewModel(part);

            updater.TryUpdateModel(model, Prefix, null, null);
            var httpContext = _httpContextAccessor.Current();

            if (httpContext.Request.Form["submit.Save"] == "submit.PublishLater") {
                if (!string.IsNullOrWhiteSpace(model.ScheduledPublishDate) && !string.IsNullOrWhiteSpace(model.ScheduledPublishTime)) {
                    DateTime scheduled;
                    
                    string parseDateTime = String.Concat(model.ScheduledPublishDate, " ", model.ScheduledPublishTime);

                    // use current culture
                    if (DateTime.TryParse(parseDateTime, _cultureInfo.Value, DateTimeStyles.None, out scheduled)) {
                        
                        // the date time is entered locally for the configured timezone
                        var timeZone = Services.WorkContext.CurrentTimeZone;

                        model.ScheduledPublishUtc = part.ScheduledPublishUtc.Value = TimeZoneInfo.ConvertTimeToUtc(scheduled, timeZone);

                        if (model.ScheduledPublishUtc < _clock.UtcNow) {
                            updater.AddModelError("ScheduledPublishUtcDate", T("You cannot schedule a publishing date in the past"));
                        }
                        else {
                            _publishLaterService.Publish(model.ContentItem, model.ScheduledPublishUtc.Value);
                        }
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