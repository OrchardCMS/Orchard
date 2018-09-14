using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.ViewModels;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Fields.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Xml;

namespace Orchard.Fields.Drivers {
    public class DateTimeFieldDriver : ContentFieldDriver<DateTimeField> {
        private const string TemplateName = "Fields/DateTime.Edit"; // EditorTemplates/Fields/DateTime.Edit.cshtml

        public DateTimeFieldDriver(IOrchardServices services, IDateLocalizationServices dateLocalizationServices) {
            Services = services;
            DateLocalizationServices = dateLocalizationServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public IDateLocalizationServices DateLocalizationServices { get; set; }
        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, DateTimeField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_DateTime", // this is just a key in the Shape Table
                GetDifferentiator(field, part),
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                    var value = field.DateTime;
                    var options = new DateLocalizationOptions();

                    // Don't do any time zone conversion if field is semantically a date-only field, because that might mutate the date component.
                    if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                        options.EnableTimeZoneConversion = false;
                    }

                    // Don't do any calendar conversion if field is semantically a time-only field, because the date component might we out of allowed boundaries for the current calendar.
                    if (settings.Display == DateTimeFieldDisplays.TimeOnly) {
                        options.EnableCalendarConversion = false;
                        options.IgnoreDate = true;
                    }

                    var showDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly;
                    var showTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly;

                    var viewModel = new DateTimeFieldViewModel {
                        Name = field.DisplayName,
                        Hint = settings.Hint,
                        IsRequired = settings.Required,
                        Editor = new DateTimeEditor() {
                            Date = showDate ? DateLocalizationServices.ConvertToLocalizedDateString(value, options) : null,
                            Time = showTime ? DateLocalizationServices.ConvertToLocalizedTimeString(value, options) : null,
                            ShowDate = showDate,
                            ShowTime = showTime,
                            DatePlaceholder = settings.DatePlaceholder,
                            TimePlaceholder = settings.TimePlaceholder
                        }
                    };

                    return shapeHelper.Fields_DateTime( // this is the actual Shape which will be resolved (Fields/DateTime.cshtml)
                        Model: viewModel);
                }
            );
        }

        protected override DriverResult Editor(ContentPart part, DateTimeField field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
            var value = part.IsNew() && field.DateTime == default(DateTime) ? settings.DefaultValue : field.DateTime;
            var options = new DateLocalizationOptions();

            // Don't do any time zone conversion if field is semantically a date-only field, because that might mutate the date component.
            if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                options.EnableTimeZoneConversion = false;
            }

            // Don't do any calendar conversion if field is semantically a time-only field, because the date component might we out of allowed boundaries for the current calendar.
            if (settings.Display == DateTimeFieldDisplays.TimeOnly) {
                options.EnableCalendarConversion = false;
                options.IgnoreDate = true;
            }

            var showDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly;
            var showTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly;

            var viewModel = new DateTimeFieldViewModel {
                Name = field.DisplayName,
                Hint = settings.Hint,
                IsRequired = settings.Required,
                Editor = new DateTimeEditor() {
                    Date = showDate ? DateLocalizationServices.ConvertToLocalizedDateString(value, options) : null,
                    Time = showTime ? DateLocalizationServices.ConvertToLocalizedTimeString(value, options) : null,
                    ShowDate = showDate,
                    ShowTime = showTime,
                    DatePlaceholder = settings.DatePlaceholder,
                    TimePlaceholder = settings.TimePlaceholder
                }
            };

            return ContentShape("Fields_DateTime_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, DateTimeField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new DateTimeFieldViewModel();

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {

                var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();

                var options = new DateLocalizationOptions();

                // Don't do any time zone conversion if field is semantically a date-only field, because that might mutate the date component.
                if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                    options.EnableTimeZoneConversion = false;
                }

                // Don't do any calendar conversion if field is semantically a time-only field, because the date component might we out of allowed boundaries for the current calendar.
                if (settings.Display == DateTimeFieldDisplays.TimeOnly) {
                    options.EnableCalendarConversion = false;
                    options.IgnoreDate = true;
                }

                var showDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly;
                var showTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly;

                DateTime? value = null;

                // Try to parse data if not required or if there are no missing fields.
                if (!settings.Required || ((!showDate || !String.IsNullOrWhiteSpace(viewModel.Editor.Date)) && (!showTime || !String.IsNullOrWhiteSpace(viewModel.Editor.Time)))) {
                    try {
                        value = DateLocalizationServices.ConvertFromLocalizedString(viewModel.Editor.Date, viewModel.Editor.Time, options);
                    }
                    catch {
                        updater.AddModelError(GetPrefix(field, part), T("{0} could not be parsed as a valid date and time.", T(field.DisplayName)));
                    }
                }

                // Hackish workaround to make sure a time-only field with an entered time equivalent to
                // 00:00 UTC doesn't get stored as a full DateTime.MinValue in the database, resulting
                // in it being interpreted as an empty value when subsequently retrieved.
                if (value.HasValue && settings.Display == DateTimeFieldDisplays.TimeOnly && value == DateTime.MinValue) {
                    value = value.Value.AddDays(1);
                }

                if (settings.Required && (!value.HasValue || (settings.Display != DateTimeFieldDisplays.TimeOnly && value.Value.Date == DateTime.MinValue))) {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is required.", T(field.DisplayName)));
                }

                field.DateTime = value.HasValue ? value.Value : DateTime.MinValue;
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, DateTimeField field, ImportContentContext context) {
            context.ImportAttribute(GetPrefix(field, part), "Value", v => field.Storage.Set(null, XmlConvert.ToDateTime(v, XmlDateTimeSerializationMode.Utc)));
        }

        protected override void Exporting(ContentPart part, DateTimeField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part)).SetAttributeValue("Value", XmlConvert.ToString(field.Storage.Get<DateTime>(null), XmlDateTimeSerializationMode.Utc));
        }

        protected override void Cloning(ContentPart part, DateTimeField originalField, DateTimeField cloneField, CloneContentContext context) {
            cloneField.DateTime = originalField.DateTime;
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(DateTime), T("Value"), T("The date and time value of the field."))
                .Enumerate<DateTimeField>(() => field => new[] { field.DateTime });
        }
    }
}
