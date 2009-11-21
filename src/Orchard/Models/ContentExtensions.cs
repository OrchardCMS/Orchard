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

        public static T Get<T>(this IContentManager manager, int id) where T : class, IContent {
            var contentItem = manager.Get(id);
            return contentItem == null ? null : contentItem.Get<T>();
        }

        public static void Create(this IContentManager manager, IContent part) {
            manager.Create(part.ContentItem);
        }

        public static bool Is<T>(this ContentItem contentItem) {
            return contentItem == null ? false : contentItem.Has(typeof(T));
        }
        public static bool Has<T>(this ContentItem contentItem) {
            return contentItem == null ? false : contentItem.Has(typeof(T));
        }
        public static T As<T>(this ContentItem contentItem) where T : class {
            return contentItem == null ? null : (T)contentItem.Get(typeof(T));
        }
        public static T Get<T>(this ContentItem contentItem) where T : class {
            return contentItem == null ? null : (T)contentItem.Get(typeof(T));
        }

        public static bool Is<T>(this IContent part) {
            return part == null ? false : part.ContentItem.Has(typeof(T));
        }
        public static bool Has<T>(this IContent part) {
            return part == null ? false : part.ContentItem.Has(typeof(T));
        }
        public static T As<T>(this IContent part) where T : class {
            return part == null ? null : (T)part.ContentItem.Get(typeof(T));
        }
        public static T Get<T>(this IContent part) where T : class {
            return part == null ? null : (T)part.ContentItem.Get(typeof(T));
        }



    }
}
