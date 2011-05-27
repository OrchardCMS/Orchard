using System;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    public class DateEditorPartDriver : ContentPartDriver<CommonPart> {
        private readonly IRepository<CommonPartVersionRecord> _commonPartVersionRecordRepository;
        private readonly IClock _clock;

        private const string DatePattern = "M/d/yyyy";
        private const string TimePattern = "h:mm tt";

        public DateEditorPartDriver(
            IOrchardServices services,
            IRepository<CommonPartVersionRecord> commonPartVersionRecordRepository,
            IClock clock) {
            _commonPartVersionRecordRepository = commonPartVersionRecordRepository;
            _clock = clock;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "DateEditorPart"; }
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {

            var commonEditorsSettings = CommonEditorsSettings.Get(part.ContentItem);
            if(!commonEditorsSettings.ShowDateEditor) {
                return null;
            }

            // this event is hooked so the modified timestamp is changed when an edit-post occurs
            part.Record.ModifiedUtc = _clock.UtcNow;

            var model = new CreatedUtcEditorViewModel();

            if (part.CreatedUtc != null) {
                // fetch CommonPartVersionRecord of first version
                var firstVersion = _commonPartVersionRecordRepository.Fetch(
                    civr => civr.ContentItemRecord == part.ContentItem.Record,
                    order => order.Asc(record => record.ContentItemVersionRecord.Number),
                    0, 1).FirstOrDefault();

                // show CreatedUtc only if is has been "touched", 
                // i.e. it has been published once, or CreatedUtc has been set
                if (firstVersion != null && firstVersion.CreatedUtc != part.CreatedUtc) {
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

            return ContentShape("Parts_Common_CreatedUtc_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Common.CreatedUtc", Model: model, Prefix: Prefix));
        }
    }
}