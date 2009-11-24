using System;

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



    }
}
