using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Localization.Services;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorDriver : ContentPartDriver<CommonPart> {
        private readonly IDateServices _dateServices;

        public DateEditorDriver(
            IOrchardServices services,
            IDateServices dateServices) {
                _dateServices = dateServices;
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

                        if (!theEditorShouldBeBlank) {
                            model.CreatedDate = _dateServices.ConvertToLocalDateString(part.CreatedUtc, "");
                            model.CreatedTime = _dateServices.ConvertToLocalTimeString(part.CreatedUtc, "");
                        }
                    }

                    if (updater != null) {
                        updater.TryUpdateModel(model, Prefix, null, null);

                        if (!String.IsNullOrWhiteSpace(model.CreatedDate) && !String.IsNullOrWhiteSpace(model.CreatedTime)) {
                            try {
                                var utcDateTime = _dateServices.ConvertFromLocalString(model.CreatedDate, model.CreatedTime);
                                part.CreatedUtc = utcDateTime;
                                part.VersionCreatedUtc = utcDateTime;
                            }
                            catch (FormatException) {
                                updater.AddModelError(Prefix, T("'{0} {1}' could not be parsed as a valid date and time.", model.CreatedDate, model.CreatedTime));                                                                             
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(model.CreatedDate) || !String.IsNullOrWhiteSpace(model.CreatedTime)) {
                            updater.AddModelError(Prefix, T("Both the date and time need to be specified."));
                        }

                        // Neither date/time part is specified => do nothing.
                    }

                    return model;
                });
        }
    }
}