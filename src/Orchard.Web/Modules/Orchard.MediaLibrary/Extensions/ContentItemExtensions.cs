using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;

namespace Orchard.ContentManagement
{
    public static class ContentItemExtensions
    {
        public static EagerlyLoadQueryResult<T> LoadMediaLibraryPickerFields<T>(this IList<T> items, IContentManager contentManager, int maximumLevel = 0) where T : class, IContent {
            var eagerlyLoadQueryResult = new EagerlyLoadQueryResult<T>(items, contentManager);
            return eagerlyLoadQueryResult.IncludeMediaLibraryPickerFields();
        }

        public static EagerlyLoadQueryResult<T> IncludeMediaLibraryPickerFields<T>(this IContentQuery<T> query) where T : class, IContent {
            var manager = query.ContentManager;
            var eagerlyLoadQueryResult = new EagerlyLoadQueryResult<T>(query.List(), manager);
            return eagerlyLoadQueryResult.IncludeMediaLibraryPickerFields();
        }

        public static EagerlyLoadQueryResult<T> IncludeMediaLibraryPickerFields<T>(this EagerlyLoadQueryResult<T> eagerlyLoadQueryResult) where T : class, IContent {
            var containerIds = new HashSet<int>();
            foreach (var part in eagerlyLoadQueryResult.Result) {
                var mediaLibraryPickerFields = part.ContentItem.Parts.SelectMany(p => p.Fields.Where(f => f is MediaLibraryPickerField).Cast<MediaLibraryPickerField>());
                var ids = mediaLibraryPickerFields.SelectMany(f => f.Ids);
                foreach (var id in ids) {
                    if (!containerIds.Contains(id))
                        containerIds.Add(id);
                }
            }
            Dictionary<int, MediaPart> containersDictionary = eagerlyLoadQueryResult.ContentManager.GetTooMany<MediaPart>(containerIds, VersionOptions.Published, QueryHints.Empty).ToDictionary(c => c.ContentItem.Id);
            foreach (var resultPart in eagerlyLoadQueryResult.Result) {
                var mediaLibraryPickerFields = resultPart.ContentItem.Parts.SelectMany(p => p.Fields.Where(f => f is MediaLibraryPickerField).Cast<MediaLibraryPickerField>());
                foreach (var mediaLibraryPickerField in mediaLibraryPickerFields) {
                    var preloadedMedias = new List<MediaPart>();
                    foreach (var mediaId in mediaLibraryPickerField.Ids) {
                        MediaPart preloadedMedia = null;
                        if (containersDictionary.TryGetValue(mediaId, out preloadedMedia))
                            preloadedMedias.Add(preloadedMedia);
                    }
                    mediaLibraryPickerField.MediaParts = preloadedMedias;
                }
            }
            return eagerlyLoadQueryResult;
        }
    }
}
