using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement {
    // Maps content identities to content items on the importer.
    public class ImportContentSession {
        private readonly IContentManager _contentManager;
        private const int BulkPage = 128;
        private int _lastIndex = 0;

        private readonly Dictionary<ContentIdentity, int> _identities;
        private readonly Dictionary<int, ContentIdentity> _contentItemIds;
        private readonly Dictionary<ContentIdentity, string> _contentTypes;

        public ImportContentSession(IContentManager contentManager) {
            _contentManager = contentManager;
            _identities = new Dictionary<ContentIdentity, int>(new ContentIdentity.ContentIdentityEqualityComparer());
            _contentItemIds = new Dictionary<int, ContentIdentity>();
            _contentTypes = new Dictionary<ContentIdentity, string>(new ContentIdentity.ContentIdentityEqualityComparer());
        }
        public void Set(string id, string contentType) {
            var contentIdentity = new ContentIdentity(id);
            _contentTypes[contentIdentity] = contentType;
        }

        public ContentItem Get(string id, string contentTypeHint = null) {
            var contentIdentity = new ContentIdentity(id);

            // lookup in local cache
            if (_identities.ContainsKey(contentIdentity)) {
                var result = _contentManager.Get(_identities[contentIdentity], VersionOptions.DraftRequired);

                // if two identities are conflicting, then ensure that there types are the same
                // e.g., importing a blog as home page (alias=) and the current home page is a page, the blog
                // won't be imported, and blog posts will be attached to the page
                if (contentTypeHint == null || result.ContentType == contentTypeHint) {
                    return result;
                }
            }

            // no result ? then check if there are some more content items to load from the db
            if(_lastIndex != int.MaxValue) {
                
                var equalityComparer = new ContentIdentity.ContentIdentityEqualityComparer();
                IEnumerable<ContentItem> block;
            
                // load identities in blocks
                while ((block = _contentManager.HqlQuery()
                    .ForVersion(VersionOptions.Latest)
                    .OrderBy(x => x.ContentItemVersion(), x => x.Asc("Id"))
                    .Slice(_lastIndex, BulkPage)).Any()) {

                        foreach (var item in block) {
                            _lastIndex++;

                            // ignore content item if it has already been imported
                            if (_contentItemIds.ContainsKey(item.Id)) {
                                continue;
                            }

                            var identity = _contentManager.GetItemMetadata(item).Identity;

                            // ignore content item if the same identity is already present
                            if (_identities.ContainsKey(identity)) {
                                continue;
                            }

                            _identities.Add(identity, item.Id);
                            _contentItemIds.Add(item.Id, identity);
                        
                            if (equalityComparer.Equals(identity, contentIdentity)) {
                                return _contentManager.Get(item.Id, VersionOptions.DraftRequired);
                            }
                        }

                    _contentManager.Flush();
                    _contentManager.Clear();
                }
            }

            _lastIndex = int.MaxValue;

            if(!_contentTypes.ContainsKey(contentIdentity)) {
                throw new ArgumentException("Unknown content type for " + id);
                
            }

            var contentItem = _contentManager.Create(_contentTypes[contentIdentity], VersionOptions.Draft);
            _identities[contentIdentity] = contentItem.Id;
            _contentItemIds[contentItem.Id] = contentIdentity;

            return contentItem;
        }
    }
}
