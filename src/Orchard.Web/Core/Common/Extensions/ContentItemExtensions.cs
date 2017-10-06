using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Core.Common.Models;

namespace Orchard.ContentManagement
{

    public class EagerlyLoadQueryResult<T>
    {
        public EagerlyLoadQueryResult(IEnumerable<T> items, IContentManager contentManager) {
            Result = items;
            ContentManager = contentManager;
        }
        public IEnumerable<T> Result { get; set; }
        public IContentManager ContentManager { get; set; }
    }
    public static class ContentItemExtensions
    {
        const int MaxPageSize = 2000;
        public static EagerlyLoadQueryResult<T> LoadContainerContentItems<T>(this IList<T> items, IContentManager contentManager, int maximumLevel = 0) where T : class, IContent {
            var eagerlyLoadQueryResult = new EagerlyLoadQueryResult<T>(items, contentManager);
            return eagerlyLoadQueryResult.IncludeContainerContentItems(maximumLevel);
        }

        public static EagerlyLoadQueryResult<T> IncludeContainerContentItems<T>(this IContentQuery<T> query, int maximumLevel = 0) where T : class, IContent{
            var manager = query.ContentManager;
            var eagerlyLoadQueryResult = new EagerlyLoadQueryResult<T>(query.List(), manager);
            return eagerlyLoadQueryResult.IncludeContainerContentItems(maximumLevel);
        }

        public static EagerlyLoadQueryResult<T> IncludeContainerContentItems<T>(this EagerlyLoadQueryResult<T> eagerlyLoadQueryResult, int maximumLevel = 0) where T : class, IContent {

            var containerIds = new HashSet<int>();
            var objectsToLoad = eagerlyLoadQueryResult.Result.ToList();
            foreach (var part in objectsToLoad)
            {
                var commonPart = part.As<CommonPart>();
                if (commonPart != null && commonPart.Record.Container != null && !containerIds.Contains(commonPart.Record.Container.Id))
                    containerIds.Add(commonPart.Record.Container.Id);
            }
            var containersDictionary = eagerlyLoadQueryResult.ContentManager.GetTooMany<IContent>(containerIds, VersionOptions.Latest, QueryHints.Empty)
                .ToDictionary(c => c.ContentItem.Id);
            foreach (var resultPart in objectsToLoad)
            {
                IContent container = null;
                var commonPart = resultPart.As<CommonPart>();
                if (commonPart == null)
                    continue;
                if (commonPart.Record.Container == null)
                    commonPart.Container = null;
                else if (containersDictionary.TryGetValue(commonPart.Record.Container.Id, out container))
                    commonPart.Container = container;
            }
            if (maximumLevel > 0 && containersDictionary.Any())
            {
                containersDictionary.Values.ToList().LoadContainerContentItems(eagerlyLoadQueryResult.ContentManager, maximumLevel - 1);
            }
            return eagerlyLoadQueryResult;
        }

        public static IEnumerable<T> GetTooMany<T>(this IContentManager contentManager, IEnumerable<int> ids, VersionOptions versionOptions, QueryHints queryHints) where T : class, IContent {
            if (ids == null)
                return null;
            var result = new List<T>();
            var nextIdList = new List<int>();

            var pageSize = MaxPageSize;
            var maxPageIndex = Math.Floor((double)ids.Count() / MaxPageSize);
            for (var page = 0; page <= maxPageIndex; page++) {
                if (maxPageIndex == page) {
                    pageSize = ids.Count() % MaxPageSize;
                }
                if (pageSize > 0) {
                    result.AddRange(contentManager.GetMany<T>(ids.Skip(2000 * page).Take(pageSize), versionOptions, queryHints));
                }
            }
            return result;
        }
    }
}
