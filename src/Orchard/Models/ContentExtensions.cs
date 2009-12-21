using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Models.ViewModels;

namespace Orchard.Models {

    public static class ContentExtensions {

        /* Item creation and accessing extension methods */

        public static T New<T>(this IContentManager manager, string contentType) where T : class, IContent {
            var contentItem = manager.New(contentType);
            if (contentItem == null)
                return null;

            var part = contentItem.Get<T>();
            if (part == null)
                throw new InvalidCastException();

            return part;
        }

        public static T Create<T>(this IContentManager manager, string contentType) where T : class, IContent {
            return manager.Create<T>(contentType, init => { });
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


        /* Display and editor convenience extension methods */

        public static ItemDisplayModel<T> BuildDisplayModel<T>(this IContentManager manager, int id, string displayType) where T : class, IContent {
            return manager.BuildDisplayModel(manager.Get<T>(id), displayType);
        }

        public static ItemEditorModel<T> BuildEditorModel<T>(this IContentManager manager, int id) where T : class, IContent {
            return manager.BuildEditorModel(manager.Get<T>(id));
        }

        public static ItemEditorModel<T> UpdateEditorModel<T>(this IContentManager manager, int id, IUpdateModel updater) where T : class, IContent {
            return manager.UpdateEditorModel(manager.Get<T>(id), updater);
        }


        /* Query related extension methods */

        public static IContentQuery<TPart> Query<TPart>(this IContentManager manager)
            where TPart : ContentPart {
            return manager.Query().ForPart<TPart>();
        }
        public static IContentQuery<TPart, TRecord> Query<TPart, TRecord>(this IContentManager manager)
            where TPart : ContentPart<TRecord>
            where TRecord : ContentPartRecord {
            return manager.Query().ForPart<TPart>().Join<TRecord>();
        }

        public static IContentQuery<ContentItem> Query(this IContentManager manager, params string[] contentTypeNames)  {
            return manager.Query().ForType(contentTypeNames);
        }
        public static IContentQuery<TPart> Query<TPart>(this IContentManager manager, params string[] contentTypeNames) where TPart : ContentPart {
            return manager.Query().ForPart<TPart>().ForType(contentTypeNames);
        }
        public static IContentQuery<TPart,TRecord> Query<TPart,TRecord>(this IContentManager manager, params string[] contentTypeNames) where TPart : ContentPart<TRecord> where TRecord : ContentPartRecord {
            return manager.Query().ForPart<TPart>().ForType(contentTypeNames).Join<TRecord>();
        }



        public static IEnumerable<T> List<T>(this IContentManager manager, params string[] contentTypeNames) where T : ContentPart {
            return manager.Query<T>(contentTypeNames).List();
        }

        public static IEnumerable<T> List<T>(this IContentQuery query) where T : IContent {
            return query.ForPart<T>().List();
        }

        public static IEnumerable<T> Slice<T>(this IContentQuery<T> query, int count) where T : IContent {
            return query.Slice(0, count);
        }


        /* Aggregate item/part type casting extension methods */

        public static bool Is<T>(this IContent content) {
            return content == null ? false : content.ContentItem.Has(typeof(T));
        }
        public static T As<T>(this IContent content) where T : IContent {
            return content == null ? default(T) : (T)content.ContentItem.Get(typeof(T));
        }

        public static bool Has<T>(this IContent content) {
            return content == null ? false : content.ContentItem.Has(typeof(T));
        }
        public static T Get<T>(this IContent content) where T : IContent {
            return content == null ? default(T) : (T)content.ContentItem.Get(typeof(T));
        }


        public static IEnumerable<T> AsPart<T>(this IEnumerable<ContentItem> items) where T : IContent {
            return items == null ? null : items.Where(item => item.Is<T>()).Select(item => item.As<T>());
        }

    }
}
