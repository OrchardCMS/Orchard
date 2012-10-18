using System;
using Orchard.Events;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Core.Shapes.Localization;
using System.Globalization;

namespace Orchard.Fields.Tokens {
    public interface ITokenProvider : IEventHandler {
        void Describe(dynamic context);
        void Evaluate(dynamic context);
    }

    public class FieldTokens : ITokenProvider {

        private readonly IClock _clock;
        private readonly IDateTimeLocalization _dateTimeLocalization;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;
        private readonly Lazy<TimeZoneInfo> _timeZone;


        public FieldTokens(
            IClock clock, 
            IDateTimeLocalization dateTimeLocalization, 
            IWorkContextAccessor workContextAccessor) {
            _clock = clock;
            _dateTimeLocalization = dateTimeLocalization;
            _workContextAccessor = workContextAccessor;

            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));
            _timeZone = new Lazy<TimeZoneInfo>(() => _workContextAccessor.GetContext().CurrentTimeZone);

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

            context.For("MediaPickerField", T("Media Picker Field"), T("Tokens for Media Picker Fields"))
                .Token("Url", T("Url"), T("The url of the media."))
                .Token("AlternateText", T("Alternate Text"), T("The alternate text of the media."))
                .Token("Class", T("Class"), T("The class of the media."))
                .Token("Style", T("Style"), T("The style of the media."))
                .Token("Alignment", T("Alignment"), T("The alignment of the media."))
                .Token("Width", T("Width"), T("The width of the media."))
                .Token("Height", T("Height"), T("The height of the media."))
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
                .Token("Date", (Func<DateTimeField, object>)(d => d.DateTime.ToString(_dateTimeLocalization.ShortDateFormat.Text, _cultureInfo.Value)))
                .Token("Time", (Func<DateTimeField, object>)(d => d.DateTime.ToString(_dateTimeLocalization.ShortTimeFormat.Text, _cultureInfo.Value)))
                .Token("DateTime", (Func<DateTimeField, object>)(d => d.DateTime.ToString(_dateTimeLocalization.ShortDateFormat.Text + " " + _dateTimeLocalization.ShortTimeFormat.Text, _cultureInfo.Value)))
                .Chain("DateTime", "Date", (Func<DateTimeField, object>)(field => field.DateTime))
                ;

            context.For<MediaPickerField>("MediaPickerField")
                .Token("Url", (Func<MediaPickerField, object>)(field => field.Url))
                .Chain("Url", "Url", (Func<MediaPickerField, object>)(field => field.Url))
                .Token("AlternateText", (Func<MediaPickerField, object>)(field => field.AlternateText))
                .Token("Class", (Func<MediaPickerField, object>)(field => field.Class))
                .Token("Style", (Func<MediaPickerField, object>)(field => field.Style))
                .Token("Alignment", (Func<MediaPickerField, object>)(field => field.Alignment))
                .Token("Width", (Func<MediaPickerField, object>)(field => field.Width))
                .Token("Height", (Func<MediaPickerField, object>)(field => field.Height))
                ;
        }
    }
}