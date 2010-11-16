using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Containers.Extensions
{
    public static class ContentQueryExtensions {
        public static IContentQuery<T> OrderBy<T>(this IContentQuery<T> query, string partAndProperty, bool descendingOrder) where T : IContent {
            //todo: (heskew) order by custom part properties
            switch (partAndProperty) {
                case "RoutePart.Title":
                    query = descendingOrder
                                ? query.OrderByDescending<RoutePartRecord, string>(record => record.Title)
                                : query.OrderBy<RoutePartRecord, string>(record => record.Title);
                    break;
                case "RoutePart.Slug":
                    query = descendingOrder
                                ? query.OrderByDescending<RoutePartRecord, string>(record => record.Slug)
                                : query.OrderBy<RoutePartRecord, string>(record => record.Slug);
                    break;
                case "ContainerCustomPart.CustomOne":
                    query = descendingOrder
                                ? query.OrderByDescending<ContainerCustomPartRecord, string>(record => record.CustomOne)
                                : query.OrderBy<ContainerCustomPartRecord, string>(record => record.CustomOne);
                    break;
                case "ContainerCustomPart.CustomTwo":
                    query = descendingOrder
                                ? query.OrderByDescending<ContainerCustomPartRecord, string>(record => record.CustomTwo)
                                : query.OrderBy<ContainerCustomPartRecord, string>(record => record.CustomTwo);
                    break;
                case "ContainerCustomPart.CustomThree":
                    query = descendingOrder
                                ? query.OrderByDescending<ContainerCustomPartRecord, string>(record => record.CustomThree)
                                : query.OrderBy<ContainerCustomPartRecord, string>(record => record.CustomThree);
                    break;
                default: // "CommonPart.PublishedUtc"
                    query = descendingOrder
                                ? query.OrderByDescending<CommonPartRecord, DateTime?>(record => record.PublishedUtc)
                                : query.OrderBy<CommonPartRecord, DateTime?>(record => record.PublishedUtc);
                    break;
            }

            return query;
        }

        public static IContentQuery<ContentItem> Where(this IContentQuery<ContentItem> query, string partAndProperty, string comparisonOperator, string comparisonValue) {
            var filterKey = string.Format("{0}|{1}", partAndProperty, comparisonOperator);
            if (!_filters.ContainsKey(filterKey))
                return query;

            return _filters[filterKey](query, comparisonValue);
        }

        // convoluted: yes; quick and works for now: yes; technical debt: not much
        private static readonly Dictionary<string, Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>> _filters = new Dictionary<string, Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>> {
            {"RoutePart.Title|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => true /* CompareTo is not implemented - r.Title.CompareTo(s) == -1*/))},
            {"RoutePart.Title|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => true /* CompareTo is not implemented - r.Title.CompareTo(s) == 1*/))},
            {"RoutePart.Title|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => r.Title.Equals(s, StringComparison.OrdinalIgnoreCase)))},
            {"RoutePart.Title|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => r.Title.StartsWith(s, StringComparison.OrdinalIgnoreCase)))},
            {"RoutePart.Slug|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => true /* CompareTo is not implemented - r.Slug.CompareTo(s) == -1*/))},
            {"RoutePart.Slug|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => true /* CompareTo is not implemented - r.Slug.CompareTo(s) == 1*/))},
            {"RoutePart.Slug|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => r.Slug.Equals(s.Trim(), StringComparison.OrdinalIgnoreCase)))},
            {"RoutePart.Slug|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<RoutePartRecord>(r => r.Slug.StartsWith(s, StringComparison.OrdinalIgnoreCase)))},
            {"CommonPart.PublishedUtc|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => r.PublishedUtc < DateTime.Parse(s)))},
            {"CommonPart.PublishedUtc|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => r.PublishedUtc > DateTime.Parse(s)))},
            {"CommonPart.PublishedUtc|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => r.PublishedUtc == DateTime.Parse(s)))}, // todo: (heskew) not practical as is. needs some sense of precision....
            {"CommonPart.PublishedUtc|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<CommonPartRecord>(r => true /* can't modified PublishedUtc for partial comparisons */))},
            // todo: (hesekw) this could benefit from a better filter implementation as this is currently very limited in functionality and I have no idea how the custom parts will be used by folks
            {"ContainerCustomPart.CustomOne|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => true /* CompareTo is not implemented - r.CustomOne.CompareTo(s) == -1*/))},
            {"ContainerCustomPart.CustomOne|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => true /* CompareTo is not implemented - r.CustomOne.CompareTo(s) == 1*/))},
            {"ContainerCustomPart.CustomOne|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => r.CustomOne.Equals(s, StringComparison.OrdinalIgnoreCase)))},
            {"ContainerCustomPart.CustomOne|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => r.CustomOne.StartsWith(s, StringComparison.OrdinalIgnoreCase)))},
            {"ContainerCustomPart.CustomTwo|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => true /* CompareTo is not implemented - r.CustomTwo.CompareTo(s) == -1*/))},
            {"ContainerCustomPart.CustomTwo|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => true /* CompareTo is not implemented - r.CustomTwo.CompareTo(s) == 1*/))},
            {"ContainerCustomPart.CustomTwo|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => r.CustomTwo.Equals(s, StringComparison.OrdinalIgnoreCase)))},
            {"ContainerCustomPart.CustomTwo|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => r.CustomTwo.StartsWith(s, StringComparison.OrdinalIgnoreCase)))},
            {"ContainerCustomPart.CustomThree|<", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => true /* CompareTo is not implemented - r.CustomThree.CompareTo(s) == -1*/))},
            {"ContainerCustomPart.CustomThree|>", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => true /* CompareTo is not implemented - r.CustomThree.CompareTo(s) == 1*/))},
            {"ContainerCustomPart.CustomThree|=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => r.CustomThree.Equals(s, StringComparison.OrdinalIgnoreCase)))},
            {"ContainerCustomPart.CustomThree|^=", new Func<IContentQuery<ContentItem>, string, IContentQuery<ContentItem>>((q, s) => q.Where<ContainerCustomPartRecord>(r => r.CustomThree.StartsWith(s, StringComparison.OrdinalIgnoreCase)))},
        };
    }
}