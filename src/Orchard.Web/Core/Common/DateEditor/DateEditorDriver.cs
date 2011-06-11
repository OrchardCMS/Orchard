using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorDriver : ContentPartDriver<CommonPart> {
        private const string DatePattern = "M/d/yyyy";
        private const string TimePattern = "h:mm:ss tt";

        public DateEditorDriver(
            IOrchardServices services) {
            T = NullLocalizer.Instance;
            Services = services;
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
                            model.CreatedDate = part.CreatedUtc.Value.ToLocalTime().ToString(DatePattern, CultureInfo.InvariantCulture);
                            model.CreatedTime = part.CreatedUtc.Value.ToLocalTime().ToString(TimePattern, CultureInfo.InvariantCulture);
                        }
                    }

                    if (updater != null) {
                        updater.TryUpdateModel(model, Prefix, null, null);

                        if (!string.IsNullOrWhiteSpace(model.CreatedDate) && !string.IsNullOrWhiteSpace(model.CreatedTime)) {
                            DateTime createdUtc;
                            string parseDateTime = String.Concat(model.CreatedDate, " ", model.CreatedTime);

                            // use an english culture as it is the one used by jQuery.datepicker by default
                            if (DateTime.TryParse(parseDateTime, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AssumeLocal, out createdUtc)) {
                                part.CreatedUtc = createdUtc.ToUniversalTime();
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