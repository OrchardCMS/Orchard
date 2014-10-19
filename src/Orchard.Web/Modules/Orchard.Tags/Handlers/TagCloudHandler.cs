using System.Linq;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Tags.Models;
using Orchard.Tags.Services;
using Orchard.Tokens;

namespace Orchard.Tags.Handlers {
    [OrchardFeature("Orchard.Tags.TagCloud")]
    public class TagCloudHandler : ContentHandler {
        private readonly ISignals _signals;
        private readonly ITokenizer _tokenizer;

        public TagCloudHandler(
            ITagCloudService tagCloudService,
            ISignals signals,
            ITokenizer tokenizer) {

            _signals = signals;
            _tokenizer = tokenizer;

            OnInitializing<TagCloudPart>((context, part) => part
                ._tagCountField.Loader(tags => {
                    var username = _tokenizer.Replace(part.Username, new object());
                    return tagCloudService.GetPopularTags(part.Buckets, part.Slug, username).ToList();
                }));

            OnPublished<TagsPart>((context, part) => InvalidateTagCloudCache());
            OnRemoved<TagsPart>((context, part) => InvalidateTagCloudCache());
            OnUnpublished<TagsPart>((context, part) => InvalidateTagCloudCache());
        }

        public void InvalidateTagCloudCache() {
            _signals.Trigger(TagCloudService.TagCloudTagsChanged);
        }
    }
}