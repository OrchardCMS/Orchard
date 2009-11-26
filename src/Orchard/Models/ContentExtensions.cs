using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Models {

    public static class ContentExtensions {
        public static T New<T>(this IContentManager manager, string contentType) where T : class, IContent {
            var contentItem = manager.New(contentType);
            if (contentItem == null)
                return null;

            var part = contentItem.Get<T>();
            if (part == null)
                throw new InvalidCastException();

            return part;
        }

        public static T Create<T>(this IContentManager manager, string contentType, Action<T> initialize) where T : class, IContent {
            var content = manager.New<T>(contentType);
            if (content == null)
                return null;

            initialize(content);
            manager.Create(content.ContentItem);
            return content;
        }

        public static T Get<T>(this IContentManager manager, int id) where T : class, IContent {
            var contentItem = manager.Get(id);
            return contentItem == null ? null : contentItem.Get<T>();
        }


        public static IContentQuery Query(this IContentManager manager, params string[] contentTypeNames)  {
            return manager.Query().ForType(contentTypeNames);
        }
        public static IEnumerable<T> List<T>(this IContentManager manager, params string[] contentTypeNames) where T : class, IContent {
            return manager.Query(contentTypeNames).List<T>();
        }

        public static IEnumerable<T> List<T>(this IContentQuery query) where T : class, IContent {
            return query.List().AsPart<T>();
        }
        public static IEnumerable<T> Slice<T>(this IContentQuery query, int skip, int count) where T : class, IContent {
            return query.Slice(skip, count).AsPart<T>();
        }
        public static IEnumerable<T> Slice<T>(this IContentQuery query, int count) where T : class, IContent {
            return query.Slice(0, count).AsPart<T>();
        }
        public static IEnumerable<ContentItem> Slice(this IContentQuery query, int count)  {
            return query.Slice(0, count);
        }



        public static bool Is<T>(this IContent content) {
            return content == null ? false : content.ContentItem.Has(typeof(T));
        }
        public static T As<T>(this IContent content) where T : class {
            return content == null ? null : (T)content.ContentItem.Get(typeof(T));
        }

        public static bool Has<T>(this IContent content) {
            return content == null ? false : content.ContentItem.Has(typeof(T));
        }
        public static T Get<T>(this IContent content) where T : class {
            return content == null ? null : (T)content.ContentItem.Get(typeof(T));
        }


        public static IEnumerable<T> AsPart<T>(this IEnumerable<ContentItem> items) where T : class {
            return items == null ? null : items.Where(item => item.Is<T>()).Select(item => item.As<T>());
        }

    }
}
