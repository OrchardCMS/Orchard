using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Services {
    [OrchardFeature("Orchard.AuditTrail.RecycleBin")]
    public class RecycleBin : IRecycleBin {
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;

        public RecycleBin(ITransactionManager transactionManager, IContentManager contentManager) {
            _transactionManager = transactionManager;
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

            var contentItems = LoadContentItems<T>(query);
            
            return new PageOfItems<T>(contentItems) {
                PageNumber = page,
                PageSize = pageSize,
                TotalItemCount = totalCount
            };
        }

        public IEnumerable<ContentItem> GetMany(IEnumerable<int> contentItemIds, QueryHints hints = null) {
            return GetMany<ContentItem>(contentItemIds, hints);
        }

        public IEnumerable<T> GetMany<T>(IEnumerable<int> contentItemIds, QueryHints hints = null) where T : class, IContent {
            var query = GetDeletedVersionsQuery(contentItemIds);
            return LoadContentItems<T>(query, hints);
        }

        public ContentItem Restore(ContentItem contentItem) {
            var versions = contentItem.Record.Versions.OrderBy(x => x.Number).ToArray();
            var lastVersion = versions.Last();

            if (lastVersion.Latest || lastVersion.Published)
                throw new InvalidOperationException(String.Format("Cannot restore content item with ID {0} ftom the recycle bin, since that item is not deleted", contentItem.Id));

            return _contentManager.Restore(contentItem, VersionOptions.Restore(lastVersion.Number, publish: false));
        }

        private IEnumerable<T> LoadContentItems<T>(IQuery query, QueryHints hints = null) where T: class, IContent {
            var rows = query.List<object>();
            var versionIds = rows.Cast<object[]>().Select(x => (int)x[0]);
            return _contentManager.GetManyByVersionId<T>(versionIds, hints ?? QueryHints.Empty);
        }

        private IQuery GetDeletedVersionsQuery(IEnumerable<int> contentItemIds = null) {
            var session = _transactionManager.GetSession();

            // Select only the highest versions where both Published and Latest are false.
            var select =
                "select max(ContentItemVersionRecord.Id), ContentItemVersionRecord.ContentItemRecord.Id, max(ContentItemVersionRecord.Number) " +
                "from Orchard.ContentManagement.Records.ContentItemVersionRecord ContentItemVersionRecord ";

            var filter = contentItemIds != null ? "where ContentItemVersionRecord.ContentItemRecord.Id in (:ids) " : default(String);

            var group =
                "group by ContentItemVersionRecord.ContentItemRecord.Id " +
                "having max(cast(Latest as Int32)) = 0 and max(cast(Published as Int32)) = 0 ";

            var hql = String.Concat(select, filter, group);
            var query = session.CreateQuery(hql);

            if (contentItemIds != null) {
                query.SetParameterList("ids", contentItemIds.ToArray());
            }
            
            return query;
        }
    }
}