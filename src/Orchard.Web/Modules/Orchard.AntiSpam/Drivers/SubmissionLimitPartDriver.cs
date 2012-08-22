using System;
using Orchard.AntiSpam.Models;
using Orchard.AntiSpam.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.AntiSpam.Drivers {
    public class SubmissionLimitPartDriver : ContentPartDriver<SubmissionLimitPart> {
        private readonly IContentManager _contentManager;
        private readonly IClock _clock;

        public SubmissionLimitPartDriver(IContentManager contentManager, IClock clock) {
            _contentManager = contentManager;
            _clock = clock;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(SubmissionLimitPart part, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(SubmissionLimitPart part, IUpdateModel updater, dynamic shapeHelper) {
            var settings = part.TypePartDefinition.Settings.GetModel<SubmissionLimitPartSettings>();

            DateTime min = DateTime.MinValue, max = DateTime.MinValue;
            var now = _clock.UtcNow;

            switch(settings.Unit) {
                case (int)SubmissionLimitUnit.Hour:
                    min = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
                    max = min.AddHours(1);
                    break;
                case (int)SubmissionLimitUnit.Day:
                    min = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                    max = min.AddDays(1);
                    break;
                case (int)SubmissionLimitUnit.Month:
                    min = new DateTime(now.Year, now.Month, 0, 0, 0, 0, DateTimeKind.Utc);
                    max = min.AddMonths(1);
                    break;
                case (int)SubmissionLimitUnit.Year:
                    min = new DateTime(now.Year, 0, 0, now.Hour, 0, 0, DateTimeKind.Utc);
                    max = min.AddYears(1);
                    break;
                case (int)SubmissionLimitUnit.Overall:
                    break;
            }

            var query = _contentManager.Query()
                .ForVersion(VersionOptions.AllVersions)
                .ForType(part.ContentItem.ContentType);

            if (min != DateTime.MinValue) {
                query = query.Join<CommonPartRecord>().Where(x => x.CreatedUtc > min && x.CreatedUtc < max);
            }

            var result = query.Count();

            if(result > settings.Limit) {
                updater.AddModelError("Limit", T("The limit of submissions has been reached."));
            }

            return null;
        }
    }
}