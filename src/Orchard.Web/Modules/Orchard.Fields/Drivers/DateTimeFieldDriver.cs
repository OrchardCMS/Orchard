using System;
using System.Globalization;
using System.Xml;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Fields.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Fields.Drivers {
    [UsedImplicitly]
    public class DateTimeFieldDriver : ContentFieldDriver<DateTimeField> {
        private const string TemplateName = "Fields/DateTime.Edit"; // EditorTemplates/Fields/DateTime.Edit.cshtml

        public DateTimeFieldDriver(IOrchardServices services, IDateServices dateServices) {
            Services = services;
            DateServices = dateServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public IDateServices DateServices { get; set; }
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

                    var viewModel = new DateTimeFieldViewModel {
                        Name = field.DisplayName,
                        Hint = settings.Hint,
                        IsRequired = settings.Required,
                        Editor = new DateTimeEditor() {
                            Date = DateServices.ConvertToLocalDateString(value, String.Empty),
                            Time = DateServices.ConvertToLocalTimeString(value, String.Empty),
                            ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                            ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly,
                        }
                    };

                    return shapeHelper.Fields_DateTime( // this is the actual Shape which will be resolved (Fields/DateTime.cshtml)
                        Model: viewModel);
                }
            );
        }

        protected override DriverResult Editor(ContentPart part, DateTimeField field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
            var value = field.DateTime;

            var viewModel = new DateTimeFieldViewModel {
                Name = field.DisplayName,
                Hint = settings.Hint,
                IsRequired = settings.Required,
                Editor = new DateTimeEditor() {
                    Date = DateServices.ConvertToLocalDateString(value, String.Empty),
                    Time = DateServices.ConvertToLocalTimeString(value, String.Empty),
                    ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                    ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly,
                }
            };

            return ContentShape("Fields_DateTime_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, DateTimeField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new DateTimeFieldViewModel();

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {

                var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                if (settings.Required && (((settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly) && String.IsNullOrWhiteSpace(viewModel.Editor.Date)) || ((settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly) && String.IsNullOrWhiteSpace(viewModel.Editor.Time)))) {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is required.", field.DisplayName));
                } else {
                    try {
                        var utcDateTime = DateServices.ConvertFromLocalString(viewModel.Editor.Date, viewModel.Editor.Time);
                        if (utcDateTime.HasValue) {
                            field.DateTime = utcDateTime.Value;
                        } else {
                            field.DateTime = DateTime.MinValue;
                        }
                    }
                    catch (FormatException) {
                        updater.AddModelError(GetPrefix(field, part), T("{0} could not be parsed as a valid date and time.", field.DisplayName));
                    }
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, DateTimeField field, ImportContentContext context) {
            context.ImportAttribute(GetPrefix(field, part), "Value", v => field.Storage.Set(null, XmlConvert.ToDateTime(v, XmlDateTimeSerializationMode.Utc)));
        }

        protected override void Exporting(ContentPart part, DateTimeField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part)).SetAttributeValue("Value", XmlConvert.ToString(field.Storage.Get<DateTime>(null), XmlDateTimeSerializationMode.Utc));
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(DateTime), T("Value"), T("The date and time value of the field."))
                .Enumerate<DateTimeField>(() => field => new[] { field.DateTime });
        }
    }
}
