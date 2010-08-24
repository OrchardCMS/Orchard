using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Services {
    public class RoutableService : IRoutableService {
        private readonly IContentManager _contentManager;

        public RoutableService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void FillSlugFromTitle<TModel>(TModel model) where TModel : RoutePart {
            if (!string.IsNullOrEmpty(model.Slug) || string.IsNullOrEmpty(model.Title))
                return;

            var slug = model.Title;
            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s]+");

            slug = dissallowed.Replace(slug, "-");
            slug = slug.Trim('-');

            if (slug.Length > 1000)
                slug = slug.Substring(0, 1000);

            model.Slug = slug.ToLowerInvariant();
        }

        public string GenerateUniqueSlug(RoutePart part, IEnumerable<string> existingPaths) {
            var slugCandidate = part.Slug;
            if (existingPaths == null || !existingPaths.Contains(part.Path))
                return slugCandidate;

            int? version = existingPaths.Select(s => GetSlugVersion(slugCandidate, s)).OrderBy(i => i).LastOrDefault();

            return version != null
                       ? string.Format("{0}-{1}", slugCandidate, version)
                       : slugCandidate;
        }

        private static int? GetSlugVersion(string slugCandidate, string slug) {
            int v;
            string[] slugParts = slug.Split(new []{slugCandidate}, StringSplitOptions.RemoveEmptyEntries);
            
            if (slugParts.Length == 0)
                return 2;

            return int.TryParse(slugParts[0].TrimStart('-'), out v)
                       ? (int?)++v
                       : null;
        }

        public IEnumerable<RoutePart> GetSimilarPaths(string path) {
            return
                _contentManager.Query().Join<RoutePartRecord>()
                    .List()
                    .Select(i => i.As<RoutePart>())
                    .Where(routable => routable.Path != null && routable.Path.StartsWith(path, StringComparison.OrdinalIgnoreCase)) // todo: for some reason the filter doesn't work within the query, even without StringComparison or StartsWith
                    .ToArray();
        }

        public bool IsSlugValid(string slug) {
            // see http://tools.ietf.org/html/rfc3987 for prohibited chars
            return slug == null || String.IsNullOrEmpty(slug.Trim()) || Regex.IsMatch(slug, @"^[^/:?#\[\]@!$&'()*+,;=\s]+$");
        }

        public bool ProcessSlug(RoutePart part) {
            FillSlugFromTitle(part);

            if (string.IsNullOrEmpty(part.Slug))
                return true;

            part.Path = part.GetPathWithSlug(part.Slug);
            var pathsLikeThis = GetSimilarPaths(part.Path);

            // Don't include *this* part in the list
            // of slugs to consider for conflict detection
            pathsLikeThis = pathsLikeThis.Where(p => p.ContentItem.Id != part.ContentItem.Id);

            //todo: (heskew) need better messages
            if (pathsLikeThis.Count() > 0) {
                var originalSlug = part.Slug;
                //todo: (heskew) make auto-uniqueness optional
                part.Slug = GenerateUniqueSlug(part, pathsLikeThis.Select(p => p.Path));

                if (originalSlug != part.Slug) {
                    return false;
                }
            }

            return true;
        }
    }
}