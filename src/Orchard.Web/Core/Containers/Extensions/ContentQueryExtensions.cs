using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;

namespace Orchard.Core.Containers.Extensions
{
    public static class ContentQueryExtensions
    {
        public static IContentQuery<ContentItem> Where(this IContentQuery<ContentItem> query, string partAndProperty, string comparisonOperator, string comparisonValue)
        {
            var filterKey = string.Format("{0}|{1}", partAndProperty, comparisonOperator);
            if (!_filters.ContainsKey(filterKey))
                return query;

            return _filters[filterKey](query, comparisonValue);
        }

        // convoluted: yes; quick and works for now: yes; technical debt: not much
        private static readonly Dictionary<string, Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>> _filters = new Dictionary<string, Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>> {
            {"TitlePart.Title|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<TitlePartRecord>(r => true /* CompareTo is not implemented - r.Title.CompareTo(s) == -1*/))},
            {"TitlePart.Title|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<TitlePartRecord>(r => true /* CompareTo is not implemented - r.Title.CompareTo(s) == 1*/))},
            {"TitlePart.Title|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<TitlePartRecord>(r => r.Title.Equals(s, StringComparison.OrdinalIgnoreCase)))},
            {"TitlePart.Title|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<TitlePartRecord>(r => r.Title.StartsWith(s, StringComparison.OrdinalIgnoreCase)))},
            {"CommonPart.PublishedUtc|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => r.PublishedUtc < DateTime.Parse(s)))},
            {"CommonPart.PublishedUtc|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => r.PublishedUtc > DateTime.Parse(s)))},
            {"CommonPart.PublishedUtc|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => r.PublishedUtc == DateTime.Parse(s)))}, // todo: (heskew) not practical as is. needs some sense of precision....
            {"CommonPart.PublishedUtc|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => true /* can't modified PublishedUtc for partial comparisons */))},
        };
    }
}
