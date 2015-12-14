using System;
using Orchard.Events;
using Orchard.Localization;
using Orchard.MediaPicker.Fields;

namespace Orchard.MediaPicker.Tokens {
    public interface ITokenProvider : IEventHandler {
        void Describe(dynamic context);
        void Evaluate(dynamic context);
    }

    public class FieldTokens : ITokenProvider {


        public FieldTokens() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic context) {
            context.For("MediaPickerField", T("Media Picker Field"), T("Tokens for Media Picker Fields"))
                .Token("Url", T("Url"), T("The url of the media."), "Url")
                .Token("AlternateText", T("Alternate Text"), T("The alternate text of the media."), "Text")
                .Token("Class", T("Class"), T("The class of the media."), "Text")
                .Token("Style", T("Style"), T("The style of the media."), "Text")
                .Token("Alignment", T("Alignment"), T("The alignment of the media."), "Text")
                .Token("Width", T("Width"), T("The width of the media."))
                .Token("Height", T("Height"), T("The height of the media."))
                ;
        }

        public void Evaluate(dynamic context) {
            context.For<MediaPickerField>("MediaPickerField")
                .Token("Url", (Func<MediaPickerField, object>)(field => field.Url))
                .Chain("Url", "Url", (Func<MediaPickerField, object>)(field => field.Url))
                .Token("AlternateText", (Func<MediaPickerField, object>)(field => field.AlternateText))
                .Chain("AlternateText", "Text", (Func<MediaPickerField, object>)(field => field.AlternateText))
                .Token("Class", (Func<MediaPickerField, object>)(field => field.Class))
                .Chain("Class", "Text", (Func<MediaPickerField, object>)(field => field.Class))
                .Token("Style", (Func<MediaPickerField, object>)(field => field.Style))
                .Chain("Style", "Text", (Func<MediaPickerField, object>)(field => field.Style))
                .Token("Alignment", (Func<MediaPickerField, object>)(field => field.Alignment))
                .Chain("Alignment", "Text", (Func<MediaPickerField, object>)(field => field.Alignment))
                .Token("Width", (Func<MediaPickerField, object>)(field => field.Width))
                .Token("Height", (Func<MediaPickerField, object>)(field => field.Height))
                ;
        }
    }
}