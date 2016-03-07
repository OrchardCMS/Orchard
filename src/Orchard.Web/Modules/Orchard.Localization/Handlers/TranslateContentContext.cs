using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization.Records;

namespace Orchard.Localization.Handlers {
    public class TranslateContentContext : CloneContentContext {
        public CultureRecord TagetCulture { get; set; }
        public TranslateContentContext(ContentItem contentItem, ContentItem cloneContentItem)
            : base(contentItem, cloneContentItem) {
        }
    }
}