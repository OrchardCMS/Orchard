using System;
using System.Globalization;
using System.Xml;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Fields.Settings;
using Orchard.Fields.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.Fields.Drivers {
    [UsedImplicitly]
    public class DateTimeFieldDriver : ContentFieldDriver<Fields.DateTimeField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/DateTime.Edit"; // EditorTemplates/Fields/DateTime.Edit.cshtml
        private readonly Lazy<CultureInfo> _cultureInfo;

        public DateTimeFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;

            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(Services.WorkContext.CurrentCulture));
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.DateTimeField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_DateTime", // this is just a key in the Shape Table
                GetDifferentiator(field, part), 
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                    var value = field.DateTime;


                    var viewModel = new DateTimeFieldViewModel {
                        Name = field.DisplayName,
                        Date = value != DateTime.MinValue ? TimeZoneInfo.ConvertTimeFromUtc(value, Services.WorkContext.CurrentTimeZone).ToString("d", _cultureInfo.Value) : String.Empty,
                        Time = value != DateTime.MinValue ? TimeZoneInfo.ConvertTimeFromUtc(value, Services.WorkContext.CurrentTimeZone).ToString("t", _cultureInfo.Value) : String.Empty,
                        ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                        ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly,
                        Hint = settings.Hint,
                        Required = settings.Required
                    };

                    return shapeHelper.Fields_DateTime( // this is the actual Shape which will be resolved (Fields/DateTime.cshtml)
                        Model: viewModel);
                }
            );
        }

        protected override DriverResult Editor(ContentPart part, Fields.DateTimeField field, dynamic shapeHelper) {
           
            var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
            var value = field.DateTime;

            var viewModel = new DateTimeFieldViewModel {
                Name = field.DisplayName,
                Date = value != DateTime.MinValue ? TimeZoneInfo.ConvertTimeFromUtc(value, Services.WorkContext.CurrentTimeZone).ToString("d", _cultureInfo.Value) : String.Empty,
                Time = value != DateTime.MinValue ? TimeZoneInfo.ConvertTimeFromUtc(value, Services.WorkContext.CurrentTimeZone).ToString("t", _cultureInfo.Value) : String.Empty,
                ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly,
                Hint = settings.Hint,
                Required = settings.Required
            };

            return ContentShape("Fields_DateTime_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part))); 
        }

        protected override DriverResult Editor(ContentPart part, Fields.DateTimeField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new DateTimeFieldViewModel();

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                DateTime value;

                var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();

                if (settings.Display == DateTimeFieldDisplays.DateOnly) {
                    viewModel.Time = new DateTime(1980, 1, 1).ToString("t", _cultureInfo.Value);
                }

                if (settings.Display == DateTimeFieldDisplays.TimeOnly) {
                    viewModel.Date = new DateTime(1980, 1, 1).ToString("d", _cultureInfo.Value);
                }

                string parseDateTime = String.Concat(viewModel.Date, " ", viewModel.Time);

                if(settings.Required && (String.IsNullOrWhiteSpace(viewModel.Time) || String.IsNullOrWhiteSpace(viewModel.Date))) {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is required", field.DisplayName));
                }

                if(!settings.Required
                    && (settings.Display != DateTimeFieldDisplays.TimeOnly && String.IsNullOrWhiteSpace(viewModel.Date))
                    || (settings.Display != DateTimeFieldDisplays.DateOnly && String.IsNullOrWhiteSpace(viewModel.Time))
                    ) {
                        field.DateTime = DateTime.MinValue;
                }
                else if (DateTime.TryParse(parseDateTime, _cultureInfo.Value, DateTimeStyles.None, out value)) {
                    field.DateTime = TimeZoneInfo.ConvertTimeToUtc(value, Services.WorkContext.CurrentTimeZone);
                }
                else {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid date and time", field.DisplayName));
                    field.DateTime = DateTime.MinValue;
                }
            }
            
            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, Fields.DateTimeField field, ImportContentContext context) {
            context.ImportAttribute(GetPrefix(field, part), "Value", v => field.Storage.Set(null, XmlConvert.ToDateTime(v, XmlDateTimeSerializationMode.Utc)));
        }

        protected override void Exporting(ContentPart part, Fields.DateTimeField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part)).SetAttributeValue("DateTime", XmlConvert.ToString(field.Storage.Get<DateTime>(null), XmlDateTimeSerializationMode.Utc));
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(DateTime), T("Value"), T("The date time value of the field."));
        }
    }
}
