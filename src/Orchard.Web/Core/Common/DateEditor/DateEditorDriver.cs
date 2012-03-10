using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Shapes.Localization;
using Orchard.Localization;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorDriver : ContentPartDriver<CommonPart> {
        private readonly IDateTimeLocalization _dateTimeLocalization;

        private readonly Lazy<CultureInfo> _cultureInfo;

        public DateEditorDriver(
            IOrchardServices services,
            IDateTimeLocalization dateTimeLocalization) {
            _dateTimeLocalization = dateTimeLocalization;
            T = NullLocalizer.Instance;
            Services = services;

            // initializing the culture info lazy initializer
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(Services.WorkContext.CurrentCulture));
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "DateEditor"; }
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var settings = part.TypePartDefinition.Settings.GetModel<DateEditorSettings>();
            if (!settings.ShowDateEditor) {
                return null;
            }

            return ContentShape(
                "Parts_Common_Date_Edit",
                () => {
                    DateEditorViewModel model = shapeHelper.Parts_Common_Date_Edit(typeof(DateEditorViewModel));

                    if (part.CreatedUtc != null) {
                        // show CreatedUtc only if is has been "touched", 
                        // i.e. it has been published once, or CreatedUtc has been set

                        var itemHasNeverBeenPublished = part.PublishedUtc == null;
                        var thisIsTheInitialVersionRecord = part.ContentItem.Version < 2;
                        var theDatesHaveNotBeenModified = part.CreatedUtc == part.VersionCreatedUtc;

                        var theEditorShouldBeBlank = 
                            itemHasNeverBeenPublished && 
                            thisIsTheInitialVersionRecord && 
                            theDatesHaveNotBeenModified;

                        if (theEditorShouldBeBlank == false) {
                            // date and time are formatted using the same patterns as DateTimePicker is, preventing other cultures issues
                            var createdLocal = TimeZoneInfo.ConvertTimeFromUtc(part.CreatedUtc.Value, Services.WorkContext.CurrentTimeZone);

                            model.CreatedDate = createdLocal.ToString(_dateTimeLocalization.ShortDateFormat.Text);
                            model.CreatedTime = createdLocal.ToString(_dateTimeLocalization.ShortTimeFormat.Text);
                        }
                    }

                    if (updater != null) {
                        updater.TryUpdateModel(model, Prefix, null, null);

                        if (!string.IsNullOrWhiteSpace(model.CreatedDate) && !string.IsNullOrWhiteSpace(model.CreatedTime)) {
                            DateTime createdUtc;
                            
                            string parseDateTime = String.Concat(model.CreatedDate, " ", model.CreatedTime);
                            var dateTimeFormat = _dateTimeLocalization.ShortDateFormat + " " + _dateTimeLocalization.ShortTimeFormat;

                            // use current culture
                            if (DateTime.TryParseExact(parseDateTime, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out createdUtc)) {

                                // the date time is entered locally for the configured timezone
                                part.CreatedUtc = TimeZoneInfo.ConvertTimeToUtc(createdUtc, Services.WorkContext.CurrentTimeZone);
                            }
                            else {
                                updater.AddModelError(Prefix, T("{0} is an invalid date and time", parseDateTime));
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(model.CreatedDate) || !string.IsNullOrWhiteSpace(model.CreatedTime)) {
                            // only one part is specified
                            updater.AddModelError(Prefix, T("Both the date and time need to be specified."));
                        }

                        // none date/time part is specified => do nothing
                    }

                    return model;
                });
        }
    }
}