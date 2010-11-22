using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Services {
    public class RoutableService : IRoutableService {
        private readonly IContentManager _contentManager;

        public RoutableService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void FixContainedPaths(IRoutableAspect part) {
            var items = _contentManager.Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(cr => cr.Container.Id == part.Id)
                .List()
                .Select(item => item.As<IRoutableAspect>()).Where(item => item != null);

            foreach (var itemRoute in items) {
                itemRoute.ContentItem.VersionRecord.Published = false; // <- to force a republish
                _contentManager.Publish(itemRoute.ContentItem);
            }
        }

        public void FillSlugFromTitle<TModel>(TModel model) where TModel : IRoutableAspect {
            if (!string.IsNullOrEmpty(model.Slug) || string.IsNullOrEmpty(model.Title))
                return;

            var slug = model.Title;
            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s\""\<\>]+");

            slug = dissallowed.Replace(slug, "-");
            slug = slug.Trim('-');

            if (slug.Length > 1000)
                slug = slug.Substring(0, 1000);

            // dots are not allowed at the begin and the end of routes
            slug = slug.Trim('.');

            model.Slug = slug.ToLowerInvariant();
        }

        public string GenerateUniqueSlug(IRoutableAspect part, IEnumerable<string> existingPaths) {
            if (existingPaths == null || !existingPaths.Contains(part.Path))
                return part.Slug;

            int? version = existingPaths.Select(s => GetSlugVersion(part.Path, s)).OrderBy(i => i).LastOrDefault();

            return version != null
                ? string.Format("{0}-{1}", part.Slug, version)
                : part.Slug;
        }

        private static int? GetSlugVersion(string path, string potentialConflictingPath) {
            int v;
            string[] slugParts = potentialConflictingPath.Split(new[] { path }, StringSplitOptions.RemoveEmptyEntries);
            
            if (slugParts.Length == 0)
                return 2;

            return int.TryParse(slugParts[0].TrimStart('-'), out v)
                       ? (int?)++v
                       : null;
        }

        public IEnumerable<IRoutableAspect> GetSimilarPaths(string path) {
            return
                _contentManager.Query().Join<RoutePartRecord>()
                    .List()
                    .Select(i => i.As<RoutePart>())
                    .Where(routable => routable.Path != null && routable.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }

        public bool IsSlugValid(string slug) {
            return String.IsNullOrWhiteSpace(slug) || Regex.IsMatch(slug, @"^[^:?#\[\]@!$&'()*+,;=\s\""\<\>]+$") && !(slug.StartsWith(".") || slug.EndsWith("."));
        }

        public bool ProcessSlug(IRoutableAspect part) {
            FillSlugFromTitle(part);

            if (string.IsNullOrEmpty(part.Slug))
                return true;

            part.Path = part.GetPathWithSlug(part.Slug);
            var pathsLikeThis = GetSimilarPaths(part.Path);

            // Don't include *this* part in the list
            // of slugs to consider for conflict detection
            pathsLikeThis = pathsLikeThis.Where(p => p.ContentItem.Id != part.ContentItem.Id);

            if (pathsLikeThis.Count() > 0) {
                var originalSlug = part.Slug;
                var newSlug = GenerateUniqueSlug(part, pathsLikeThis.Select(p => p.Path));
                part.Path = part.GetPathWithSlug(newSlug);
                part.Slug = newSlug;

                if (originalSlug != newSlug)
                    return false;
            }

            return true;
        }
    }

    public static class RoutableAspectExtensions {
        public static string GetContainerPath(this IRoutableAspect routableAspect) {
            var commonAspect = routableAspect.As<ICommonPart>();
            if (commonAspect != null && commonAspect.Container != null) {
                var routable = commonAspect.Container.As<IRoutableAspect>();
                if (routable != null)
                    return routable.Path;
            }
            return null;
        }

        public static string GetPathWithSlug(this IRoutableAspect routableAspect, string slug) {
            var containerPath = routableAspect.GetContainerPath();
            return !string.IsNullOrEmpty(containerPath)
                ? string.Format("{0}/{1}", containerPath, slug)
                : slug;
        }

        public static string GetChildPath(this IRoutableAspect routableAspect, string slug) {
            return string.Format("{0}/{1}", routableAspect.Path, slug);
        }

        public static string GetEffectiveSlug(this IRoutableAspect routableAspect) {
            var containerPath = routableAspect.GetContainerPath();

            if (string.IsNullOrWhiteSpace(routableAspect.Path))
                return "";

            var slugParts = routableAspect.Path.Split(new []{string.Format("{0}/", containerPath)}, StringSplitOptions.RemoveEmptyEntries);
            return slugParts.FirstOrDefault();
        }
    }
}