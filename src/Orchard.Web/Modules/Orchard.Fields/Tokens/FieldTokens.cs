using System;
using Orchard.Events;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Localization.Services;
using System.Globalization;

namespace Orchard.Fields.Tokens {
    public interface ITokenProvider : IEventHandler {
        void Describe(dynamic context);
        void Evaluate(dynamic context);
    }

    public class FieldTokens : ITokenProvider {

        private readonly IDateTimeFormatProvider _dateTimeLocalization;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public FieldTokens(
            IDateTimeFormatProvider dateTimeLocalization, 
            IWorkContextAccessor workContextAccessor) {
            _dateTimeLocalization = dateTimeLocalization;
            _workContextAccessor = workContextAccessor;

            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic context) {

            context.For("LinkField", T("Link Field"), T("Tokens for Link Fields"))
                .Token("Text", T("Text"), T("The text of the link."))
                .Token("Url", T("Url"), T("The url of the link."))
                .Token("Target", T("Target"), T("The target of the link."))
                ;

            context.For("DateTimeField", T("Date Time Field"), T("Tokens for Date Time Fields"))
                .Token("Date", T("Date"), T("The date only."))
                .Token("Time", T("Time"), T("The time only."))
                .Token("DateTime", T("Date Time"), T("The date and time."))
                ;
        }

        public void Evaluate(dynamic context) {
            context.For<LinkField>("LinkField")
                .Token("Text", (Func<LinkField, object>)(field => field.Text))
                .Chain("Text", "Text", (Func<LinkField, object>)(field => field.Text))
                .Token("Url", (Func<LinkField, object>)(field => field.Value))
                .Chain("Url", "Url", (Func<LinkField, object>)(field => field.Value))
                .Token("Target", (Func<LinkField, object>)(field => field.Target))
                ;

            context.For<DateTimeField>("DateTimeField")
                .Token("Date", (Func<DateTimeField, object>)(d => d.DateTime.ToString(_dateTimeLocalization.ShortDateFormat, _cultureInfo.Value)))
                .Token("Time", (Func<DateTimeField, object>)(d => d.DateTime.ToString(_dateTimeLocalization.ShortTimeFormat, _cultureInfo.Value)))
                .Token("DateTime", (Func<DateTimeField, object>)(d => d.DateTime.ToString(_dateTimeLocalization.ShortDateTimeFormat, _cultureInfo.Value)))
                .Chain("DateTime", "Date", (Func<DateTimeField, object>)(field => field.DateTime))
                ;
        }
    }
}