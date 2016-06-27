using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Utilities;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Services;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorDriver : ContentPartDriver<CommonPart> {
        private readonly IDateLocalizationServices _dateLocalizationServices;

        public DateEditorDriver(
            IOrchardServices services,
            IDateLocalizationServices dateLocalizationServices) {
                _dateLocalizationServices = dateLocalizationServices;
                T = NullLocalizer.Instance;
                Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return ""; }
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
                    
                    model.Editor = new DateTimeEditor() {
                        ShowDate = true,
                        ShowTime = true
                    };

                    if (part.CreatedUtc != null) {
                        // show CreatedUtc only if is has been "touched", 
                        // i.e. it has been published once, or CreatedUtc has been set

                        var itemHasNeverBeenPublished = part.PublishedUtc == null;
                        var thisIsTheInitialVersionRecord = part.ContentItem.Version < 2;

                        // Dates are assumed the same if the millisecond part is the only difference.
                        // This is because SqlCe doesn't support high precision times (Datetime2) and the infoset does
                        // implying some discrepancies between values read from different storage mechanism.
                        var theDatesHaveNotBeenModified = DateUtils.DatesAreEquivalent(part.CreatedUtc, part.VersionCreatedUtc);

                        var theEditorShouldBeBlank =
                            itemHasNeverBeenPublished &&
                            thisIsTheInitialVersionRecord &&
                            theDatesHaveNotBeenModified;

                        if (!theEditorShouldBeBlank) {
                            model.Editor.Date = _dateLocalizationServices.ConvertToLocalizedDateString(part.CreatedUtc);
                            model.Editor.Time = _dateLocalizationServices.ConvertToLocalizedTimeString(part.CreatedUtc);
                        }
                    }

                    if (updater != null) {
                        updater.TryUpdateModel(model, Prefix, null, null);

                        if (!String.IsNullOrWhiteSpace(model.Editor.Date) && !String.IsNullOrWhiteSpace(model.Editor.Time)) {
                            try {
                                var utcDateTime = _dateLocalizationServices.ConvertFromLocalizedString(model.Editor.Date, model.Editor.Time);
                                part.CreatedUtc = utcDateTime;
                            }
                            catch (FormatException) {
                                updater.AddModelError(Prefix, T("'{0} {1}' could not be parsed as a valid date and time.", model.Editor.Date, model.Editor.Time));
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(model.Editor.Date) || !String.IsNullOrWhiteSpace(model.Editor.Time)) {
                            updater.AddModelError(Prefix, T("Both the date and time need to be specified."));
                        }

                        // Neither date/time part is specified => do nothing.
                    }

                    return model;
                });
        }

    }
}