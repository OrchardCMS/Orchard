using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.Services {
    public class RoutableService : IRoutableService {
        private readonly IContentManager _contentManager;

        public RoutableService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void FillSlug<TModel>(TModel model) where TModel : IsRoutable {
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

        public void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : IsRoutable {
            if (!string.IsNullOrEmpty(model.Slug) || string.IsNullOrEmpty(model.Title))
                return;

            model.Slug = generateSlug(model.Title).ToLowerInvariant();
        }

        public string GenerateUniqueSlug(string slugCandidate, IEnumerable<string> existingSlugs) {
            if (existingSlugs == null || !existingSlugs.Contains(slugCandidate))
                return slugCandidate;

            int? version = existingSlugs.Select(s => GetSlugVersion(slugCandidate, s)).OrderBy(i => i).LastOrDefault();

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

        public IEnumerable<IsRoutable> GetSimilarSlugs(string contentType, string slug)
        {
            return
                _contentManager.Query().Join<RoutableRecord>()
                    .List()
                    .Select(i => i.As<IsRoutable>())
                    .Where(routable => routable.Path != null && routable.Path.Equals(slug, StringComparison.OrdinalIgnoreCase)) // todo: for some reason the filter doesn't work within the query, even without StringComparison or StartsWith
                    .ToArray();
        }

        public bool IsSlugValid(string slug) {
            // see http://tools.ietf.org/html/rfc3987 for prohibited chars
            return slug == null || String.IsNullOrEmpty(slug.Trim()) || Regex.IsMatch(slug, @"^[^/:?#\[\]@!$&'()*+,;=\s]+$");
        }

        public bool ProcessSlug(IsRoutable part)
        {
            FillSlug(part);

            if (string.IsNullOrEmpty(part.Slug))
            {
                return true;
            }

            var slugsLikeThis = GetSimilarSlugs(part.ContentItem.ContentType, part.Path);

            // If the part is already a valid content item, don't include it in the list
            // of slug to consider for conflict detection
            if (part.ContentItem.Id != 0)
                slugsLikeThis = slugsLikeThis.Where(p => p.ContentItem.Id != part.ContentItem.Id);

            //todo: (heskew) need better messages
            if (slugsLikeThis.Count() > 0)
            {
                var originalSlug = part.Slug;
                //todo: (heskew) make auto-uniqueness optional
                part.Slug = GenerateUniqueSlug(part.Slug, slugsLikeThis.Select(p => p.Slug));

                if (originalSlug != part.Slug) {
                    return false;
                }
            }

            return true;
        }
    }
}