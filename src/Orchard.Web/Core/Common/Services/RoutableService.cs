using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Core.Common.Services {
    [UsedImplicitly]
    public class RoutableService : IRoutableService {
        private readonly IOrchardServices _services;
        private readonly IContentManager _contentManager;
        private Localizer T { get; set; }

        public RoutableService(IOrchardServices services, IContentManager contentManager) {
            _services = services;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public void FillSlug<TModel>(TModel model) where TModel : RoutableAspect {
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

        public void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : RoutableAspect {
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

        public string[] GetSimilarSlugs(string contentType, string slug)
        {
            return
                _contentManager.Query(contentType).Join<RoutableRecord>()
                    .Where(rr => rr.Slug.StartsWith(slug, StringComparison.OrdinalIgnoreCase))
                    .List()
                    .Cast<RoutableRecord>()
                    .Select(i => i.Slug)
                    .ToArray();
        }

        public bool IsSlugValid(string slug) {
            return slug == null || String.IsNullOrEmpty(slug.Trim()) || !Regex.IsMatch(slug, @"^[^/:?#\[\]@!$&'()*+,;=\s]+$");
        }

        public void ProcessSlug(RoutableAspect part)
        {
            FillSlug(part);

            if (string.IsNullOrEmpty(part.Slug))
            {
                return;
            }

            var slugsLikeThis = GetSimilarSlugs(part.ContentItem.ContentType, part.Slug);

            //todo: (heskew) need better messages
            if (slugsLikeThis.Length > 0)
            {
                var originalSlug = part.Slug;
                //todo: (heskew) make auto-uniqueness optional
                part.Slug = GenerateUniqueSlug(part.Slug, slugsLikeThis);

                if (originalSlug != part.Slug) {
                    _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                            originalSlug, part.Slug, part.ContentItem.ContentType));
                }
            }
        }
    }
}