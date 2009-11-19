using System;

namespace Orchard.Models {

    public static class ContentExtensions {
        public static T New<T>(this IContentManager manager, string contentType) where T : class, IContentItemPart {
            var contentItem = manager.New(contentType);
            if (contentItem == null)
                return null;

            var part = contentItem.Get<T>();
            if (part == null)
                throw new InvalidCastException();

            return part;
        }

        public static T Get<T>(this IContentManager manager, int id) where T : class, IContentItemPart {
            return manager.Get(id).Get<T>();
        }

        public static void Create(this IContentManager manager, IContentItemPart part) {
            manager.Create(part.ContentItem);
        }

        public static bool Has<T>(this IContentItemPart part) {
            return part.ContentItem.Has<T>();
        }

        public static T Get<T>(this IContentItemPart part) {
            return part.ContentItem.Get<T>();
        }

        public static bool Is<T>(this IContentItemPart part) {
            return part.ContentItem.Has<T>();
        }

        public static T As<T>(this IContentItemPart part) {
            return part.ContentItem.Get<T>();
        }

        public static bool Is<T>(this ContentItem contentItem) {
            return contentItem.Has<T>();
        }
        public static T As<T>(this ContentItem contentItem) {
            return contentItem.Get<T>();
        }

    }
}
