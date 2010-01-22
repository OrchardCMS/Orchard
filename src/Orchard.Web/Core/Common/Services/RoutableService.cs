using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Services {
    public class RoutableService : IRoutableService {
        public void FillSlug<TModel>(TModel model) where TModel : RoutableAspect {
            if (!string.IsNullOrEmpty(model.Slug) || string.IsNullOrEmpty(model.Title))
                return;

            var slug = model.Title;
            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s]+");

            slug = dissallowed.Replace(slug, "-");
            slug = slug.Trim('-');

            if (slug.Length > 1000)
                slug = slug.Substring(0, 1000);

            model.Slug = slug;
        }

        public void FillSlug<TModel>(TModel model, Func<string, string> generateSlug) where TModel : RoutableAspect {
            if (!string.IsNullOrEmpty(model.Slug) || string.IsNullOrEmpty(model.Title))
                return;

            model.Slug = generateSlug(model.Title);
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
    }
}