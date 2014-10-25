using System.Linq;
using NHibernate;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;

namespace Orchard.AuditTrail.Services {
    public class RecycleBin : IRecycleBin {
        private readonly ISessionLocator _sessionLocator;
        private readonly IContentManager _contentManager;

        public RecycleBin(ISessionLocator sessionLocator, IContentManager contentManager) {
            _sessionLocator = sessionLocator;
            _contentManager = contentManager;
        }

        public IPageOfItems<ContentItem> List(int page, int pageSize) {
            return List<ContentItem>(page, pageSize);
        }

        public IPageOfItems<T> List<T>(int page, int pageSize) where T: class, IContent {
            var query = GetDeletedVersionsQuery();
            var totalCount = query.List().Count;

            query.SetFirstResult((page - 1) * pageSize);
            query.SetFetchSize(pageSize);

            var rows = query.List<object>();
            var versionIds = rows.Cast<object[]>().Select(x => (int)x[0]);
            var contentItems = _contentManager.GetManyByVersionId<T>(versionIds, QueryHints.Empty);
            
            return new PageOfItems<T>(contentItems) {
                PageNumber = page,
                PageSize = pageSize,
                TotalItemCount = totalCount
            };
        }

        public ContentItem Restore(ContentItem contentItem) {
            var versions = contentItem.Record.Versions.OrderBy(x => x.Number).ToArray();
            var lastVersion = versions.Last();
            return _contentManager.Restore(contentItem, VersionOptions.Restore(lastVersion.Number, publish: false));
        }

        private IQuery GetDeletedVersionsQuery() {
            var session = _sessionLocator.For(typeof(ContentItemVersionRecord));

            // Select only the highest versions where both Published and Latest are false.
            var query = session.CreateQuery(
                "select max(ContentItemVersionRecord.Id), ContentItemVersionRecord.ContentItemRecord.Id, max(ContentItemVersionRecord.Number) " +
                "from Orchard.ContentManagement.Records.ContentItemVersionRecord ContentItemVersionRecord " +
                "join ContentItemVersionRecord.ContentItemRecord ContentItemRecord " +
                "group by ContentItemVersionRecord.ContentItemRecord.Id " +
                "having max(cast(Latest as Int32)) = 0 and max(cast(Published AS Int32)) = 0 ");

            return query;
        }
    }
}