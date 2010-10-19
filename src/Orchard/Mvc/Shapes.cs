using System;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Services;

namespace Orchard.Mvc {
    public class Shapes {
        private readonly IClock _clock;

        public Shapes(IClock clock) {
            _clock = clock;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public LocalizedString DateTimeRelative(HtmlHelper Html, DateTime? dateTime, LocalizedString defaultIfNull) {
            return dateTime.HasValue ? DateTimeRelative(Html, dateTime.Value) : defaultIfNull;
        }

        [Shape]
        public LocalizedString DateTimeRelative(HtmlHelper Html, DateTime dateTime) {
            var time = _clock.UtcNow - dateTime;

            if (time.TotalDays > 7)
                return Html.DateTime(dateTime, T("'on' MMM d yyyy 'at' h:mm tt"));
            if (time.TotalHours > 24)
                return T.Plural("1 day ago", "{0} days ago", time.Days);
            if (time.TotalMinutes > 60)
                return T.Plural("1 hour ago", "{0} hours ago", time.Hours);
            if (time.TotalSeconds > 60)
                return T.Plural("1 minute ago", "{0} minutes ago", time.Minutes);
            if (time.TotalSeconds > 10)
                return T.Plural("1 second ago", "{0} seconds ago", time.Seconds); //aware that the singular won't be used

            return T("a moment ago");
        }
    }
}
