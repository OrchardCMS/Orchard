using System;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using Orchard.PublishLater.ViewModels;
using Orchard.Services;
using Orchard.Tasks.Scheduling;

namespace Orchard.PublishLater.Drivers {
    public class PublishLaterPartDriver : ContentPartDriver<PublishLaterPart> {
        private const string TemplateName = "Parts/PublishLater";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPublishLaterService _publishLaterService;
        private readonly IClock _clock;
        private readonly IDateLocalizationServices _dateLocalizationServices;
        private readonly IPublishingTaskManager _publishingTaskManager;

        public PublishLaterPartDriver(
            IOrchardServices services,
            IHttpContextAccessor httpContextAccessor,
            IPublishLaterService publishLaterService,
            IClock clock,
            IDateLocalizationServices dateLocalizationServices,
            IPublishingTaskManager publishingTaskManager) {
            _httpContextAccessor = httpContextAccessor;
            _publishLaterService = publishLaterService;
            _clock = clock;
            _dateLocalizationServices = dateLocalizationServices;
            T = NullLocalizer.Instance;
            Services = services;
            _publishingTaskManager = publishingTaskManager;
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
                return "PublishLater";
            }
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

        private PublishLaterViewModel BuildViewModelFromPart(PublishLaterPart part) {
            return new PublishLaterViewModel(part) {
                Editor = new DateTimeEditor() {
                    ShowDate = true,
                    ShowTime = true,
                    Date = !part.IsPublished() ? _dateLocalizationServices.ConvertToLocalizedDateString(part.ScheduledPublishUtc.Value) : "",
                    Time = !part.IsPublished() ? _dateLocalizationServices.ConvertToLocalizedTimeString(part.ScheduledPublishUtc.Value) : "",
                }
            };
        }

        protected override DriverResult Editor(PublishLaterPart part, dynamic shapeHelper) {
            var model = BuildViewModelFromPart(part);

            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(PublishLaterPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildViewModelFromPart(part);

            updater.TryUpdateModel(model, Prefix, null, null);
            var httpContext = _httpContextAccessor.Current();
            if (httpContext.Request.Form["submit.Save"] == "submit.PublishLater") {
                if (!String.IsNullOrWhiteSpace(model.Editor.Date) && !String.IsNullOrWhiteSpace(model.Editor.Time)) {
                    try {
                        var utcDateTime = _dateLocalizationServices.ConvertFromLocalizedString(model.Editor.Date, model.Editor.Time);
                        if (utcDateTime.HasValue) {
                            if (utcDateTime.Value < _clock.UtcNow) {
                                updater.AddModelError("ScheduledPublishUtcDate", T("You cannot schedule a publishing date in the past."));
                            }
                            else {
                                _publishLaterService.Publish(model.ContentItem, utcDateTime.Value);
                            }
                        }
                    }
                    catch (FormatException) {
                        updater.AddModelError(Prefix, T("'{0} {1}' could not be parsed as a valid date and time.", model.Editor.Date, model.Editor.Time));                                             
                    }
                }
                else {
                    updater.AddModelError(Prefix, T("Both the date and time need to be specified for when this is to be published. If you don't want to schedule publishing then click Save Draft or Publish."));
                }
            }

            if (httpContext.Request.Form["submit.Save"] == "submit.CancelPublishLaterTasks") {
                _publishingTaskManager.DeleteTasks(model.ContentItem);
            }
            return ContentShape("Parts_PublishLater_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override void Importing(PublishLaterPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "ScheduledPublishUtc", scheduledUtc =>
                part.ScheduledPublishUtc.Value = XmlConvert.ToDateTime(scheduledUtc, XmlDateTimeSerializationMode.Utc)
            );
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