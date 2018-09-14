using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Orchard.Fields.Settings {
    public class DateTimeFieldEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IDateLocalizationServices _dateLocalizationServices;

        public DateTimeFieldEditorEvents(IDateLocalizationServices dateLocalizationServices) {
            _dateLocalizationServices = dateLocalizationServices;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "DateTimeField") {
                var model = definition.Settings.GetModel<DateTimeFieldSettings>();
                model.Editor = InitialDateTimeEditor(model.DefaultValue);
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "DateTimeField") {
                yield break;
            }

            var model = new DateTimeFieldSettings();
            if(updateModel.TryUpdateModel(model, "DateTimeFieldSettings", null, null)) {
                builder.WithSetting("DateTimeFieldSettings.Display", model.Display.ToString());
                builder.WithSetting("DateTimeFieldSettings.Hint", model.Hint);
                builder.WithSetting("DateTimeFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("DateTimeFieldSettings.DatePlaceholder", model.DatePlaceholder);
                builder.WithSetting("DateTimeFieldSettings.TimePlaceholder", model.TimePlaceholder);
                model.DefaultValue = model.Editor == null ? model.DefaultValue : _dateLocalizationServices.ConvertFromLocalizedString(model.Editor.Date, model.Editor.Time);
                builder.WithSetting("DateTimeFieldSettings.DefaultValue", model.DefaultValue.HasValue ? model.DefaultValue.Value.ToString(CultureInfo.InvariantCulture) : String.Empty);
                model.Editor = InitialDateTimeEditor(model.DefaultValue, model.Display);
                yield return DefinitionTemplate(model);
            }
        }
        
        private DateTimeEditor InitialDateTimeEditor(DateTime? value,  DateTimeFieldDisplays displays = DateTimeFieldDisplays.DateAndTime)
        {
            var showDate = displays == DateTimeFieldDisplays.DateAndTime || displays == DateTimeFieldDisplays.DateOnly;
            var showTime = displays == DateTimeFieldDisplays.DateAndTime || displays == DateTimeFieldDisplays.TimeOnly;
            var editor = new DateTimeEditor()
            {
                ShowDate = showDate,
                ShowTime = showTime,
                Date = value != null ? _dateLocalizationServices.ConvertToLocalizedDateString(value) : null,
                Time = value != null ? _dateLocalizationServices.ConvertToLocalizedTimeString(value) : null
            };
            return editor;
        }
    }
}
