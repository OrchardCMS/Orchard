using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;
using Orchard.Tokens;

namespace Orchard.MediaLibrary.Tokens {

    public class FieldTokens : ITokenProvider {
        private readonly IContentManager _contentManager;


        public FieldTokens(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("MediaLibraryPickerField", T("First media for Media Library Picker Field"), T("Tokens for Media Picker Fields"))
                .Token("Url", T("Url"), T("The url of the media."), "Url")
                ;
        }

        public void Evaluate(EvaluateContext context) {
            context.For<MediaLibraryPickerField>("MediaLibraryPickerField")
                .Token("Url", field => {
                    var mediaId = field.Ids.FirstOrDefault();
                    var media = _contentManager.Get<MediaPart>(mediaId);
                    return media == null ? "" : media.MediaUrl;
                })
                .Chain("Url", "Url", field => {
                    var mediaId = field.Ids.FirstOrDefault();
                    var media = _contentManager.Get<MediaPart>(mediaId);
                    return media == null ? "" : media.MediaUrl;
                })
                ;
        }
    }
}