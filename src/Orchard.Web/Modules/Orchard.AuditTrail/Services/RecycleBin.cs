using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Services
{
    [OrchardFeature("Orchard.AuditTrail.RecycleBin")]
    public class RecycleBin : IRecycleBin
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellSettings _shellSettings;

        public RecycleBin(
            ITransactionManager transactionManager,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ShellSettings shellSettings)
        {
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _shellSettings = shellSettings;
        }

        public IPageOfItems<ContentItem> List(int page, int pageSize, string contentTypeName = null)
        {
            return List<ContentItem>(page, pageSize, contentTypeName);
        }

        public IPageOfItems<T> List<T>(int page, int pageSize, string contentTypeName = null) where T : class, IContent
        {
            var query = GetDeletedVersionsQuery(null, contentTypeName);
            var totalCount = query.List().Count;

            query.SetFirstResult((page - 1) * pageSize);
            query.SetMaxResults(pageSize);

            var contentItems = LoadContentItems<T>(query);

            return new PageOfItems<T>(contentItems)
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalItemCount = totalCount
            };
        }

        public IEnumerable<ContentItem> GetMany(IEnumerable<int> contentItemIds, QueryHints hints = null, string contentTypeName = null) =>
            GetMany<ContentItem>(contentItemIds, hints, contentTypeName);

        public IEnumerable<T> GetMany<T>(IEnumerable<int> contentItemIds, QueryHints hints = null, string contentTypeName = null)
            where T : class, IContent
        {
            var query = GetDeletedVersionsQuery(contentItemIds, contentTypeName);
            return LoadContentItems<T>(query, hints);
        }

        public ContentItem Restore(ContentItem contentItem)
        {
            var versions = contentItem.Record.Versions.OrderBy(x => x.Number).ToArray();
            var lastVersion = versions.Last();

            return lastVersion.Latest || lastVersion.Published
                ? throw new InvalidOperationException(
                    string.Format("Cannot restore content item with ID {0} from the recycle bin, since that item is not deleted.", contentItem.Id))
                : _contentManager.Restore(contentItem, VersionOptions.Restore(lastVersion.Number, publish: false));
        }

        private IEnumerable<T> LoadContentItems<T>(IQuery query, QueryHints hints = null) where T : class, IContent
        {
            var rows = query.List<object>();
            var versionIds = rows.Cast<object[]>().Select(x => (int)x[0]);
            return _contentManager.GetManyByVersionId<T>(versionIds, hints ?? QueryHints.Empty);
        }

        private IQuery GetDeletedVersionsQuery(IEnumerable<int> contentItemIds = null, string contentTypeName = null)
        {
            var session = _transactionManager.GetSession();

            // Select only the highest versions where both Published and Latest are false.
            var select = @"
                select
                    max(contentItemVersionRecord.Id),
                    max(contentItemVersionRecord.Number),
                    contentItemVersionRecord.ContentItemRecord.Id
                from
                    Orchard.ContentManagement.Records.ContentItemVersionRecord contentItemVersionRecord
                left join
                    Orchard.Core.Common.Models.CommonPartVersionRecord commonPartVersionRecord
                on
                    contentItemVersionRecord.Id = commonPartVersionRecord.Id";

            var filter = contentItemIds == null
                ? default
                : "\nwhere contentItemVersionRecord.ContentItemRecord.Id in (:ids)";

            // ContentTypeName is safe to use in a query directly, because it's a technical name without special characters.
            if (contentTypeName != null
                && _contentDefinitionManager.ListTypeDefinitions().Any(typeDefinition => typeDefinition.Name == contentTypeName))
            {
                filter += $@"
                    {(filter == default ? "where" : "and")}
                        contentItemVersionRecord.ContentItemRecord.ContentType.Name = '{contentTypeName}'";
            }

            var group = @"
                group by
                    contentItemVersionRecord.ContentItemRecord.Id
                having
                    max(cast(Latest as Int32)) = 0
                    and max(cast(Published as Int32)) = 0";

            var order = "";
            // SQL CE doesn't support ordering by an aggregate value.
            if (!string.Equals(_shellSettings.DataProvider, "SqlCe", StringComparison.OrdinalIgnoreCase))
            {
                order = @"
                    order by
                        max(commonPartVersionRecord.ModifiedUtc) desc";
            }

            var hql = string.Concat(select, filter, group, order);
            var query = session.CreateQuery(hql);

            if (contentItemIds != null)
            {
                query.SetParameterList("ids", contentItemIds.ToArray());
            }

            return query;
        }
    }
}
