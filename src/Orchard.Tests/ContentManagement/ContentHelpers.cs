using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement {
    public class ContentHelpers {
        public static ContentItem PreparePart<TPart, TRecord>(TPart part, string contentType, int id = -1)
            where TPart : ContentPart<TRecord>
            where TRecord : ContentPartRecord, new() {

            part.Record = new TRecord();
            return PreparePart(part, contentType, id);
        }

        public static ContentItem PreparePart<TPart>(TPart part, string contentType, int id = -1)
            where TPart : ContentPart {

            var contentItem = part.ContentItem = new ContentItem {
                VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord()
                },
                ContentType = contentType
            };
            contentItem.Record.Id = id;
            contentItem.Weld(part);
            contentItem.Weld(new InfosetPart());
            return contentItem;
        }
    }
}
