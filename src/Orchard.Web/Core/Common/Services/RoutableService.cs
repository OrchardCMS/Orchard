using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orchard.Core.Common.Services {
    public class RoutableService : IRoutableService {
        public string Slugify(string title) {
            if (!string.IsNullOrEmpty(title)) {
                //todo: (heskew) improve - just doing multi-pass regex replaces for now with the simple rules of
                // (1) can't begin with a '/', (2) can't have adjacent '/'s and (3) can't have these characters
                var startsoffbad = new Regex(@"^[\s/]+");
                var slashhappy = new Regex("/{2,}");
                var dissallowed = new Regex(@"[:?#\[\]@!$&'()*+,;=\s]+");

                title = title.Trim();
                title = startsoffbad.Replace(title, "-");
                title = slashhappy.Replace(title, "/");
                title = dissallowed.Replace(title, "-");

                if (title.Length > 1000) {
                    title = title.Substring(0, 1000);
                }
            }

            return title;
        }

        public string GenerateUniqueSlug(string slugCandidate, IEnumerable<string> existingSlugs) {
            int? version = existingSlugs
                .Select(s => {
                            int v;
                            string[] slugParts = s.Split(new[] { slugCandidate }, StringSplitOptions.RemoveEmptyEntries);
                            if (slugParts.Length == 0) {
                                return 1;
                            }

                            return int.TryParse(slugParts[0].TrimStart('-'), out v)
                                       ? (int?) ++v
                                       : null;
                        })
                .OrderBy(i => i)
                .LastOrDefault();

            return version != null
                       ? string.Format("{0}-{1}", slugCandidate, version)
                       : slugCandidate;
        }
    }
}