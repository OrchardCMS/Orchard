using System;
using System.Globalization;
using System.Text;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services {

    public class DefaultSlugService : ISlugService {

        private readonly ISlugEventHandler _slugEventHandler;

        public DefaultSlugService(
            ISlugEventHandler slugEventHander
            ) {
                _slugEventHandler = slugEventHander;
        }

        public string Slugify(IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata == null || String.IsNullOrEmpty(metadata.DisplayText)) return null;
            var title = metadata.DisplayText.Trim();
            return Slugify(new FillSlugContext(content,title));
        }

        private string Slugify(FillSlugContext slugContext) {
            _slugEventHandler.FillingSlugFromTitle(slugContext);

            if (!slugContext.Adjusted) {
                string stFormKD = slugContext.Title.ToLower().Normalize(NormalizationForm.FormKD);
                var sb = new StringBuilder();

                foreach (char t in stFormKD) {
                    // Allowed symbols
                    if (t == '-' || t == '_' || t == '~') {
                        sb.Append(t);
                        continue;
                    }

                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(t);
                    switch (uc) {
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.OtherLetter:
                        case UnicodeCategory.DecimalDigitNumber:
                            // Keep letters and digits
                            sb.Append(t);
                            break;
                        case UnicodeCategory.NonSpacingMark:
                            // Remove diacritics
                            break;
                        default:
                            // Replace all other chars with dash
                            sb.Append('-');
                            break;
                    }
                }

                slugContext.Slug = sb.ToString().Normalize(NormalizationForm.FormC);

                // Simplifies dash groups 
                for (int i = 0; i < slugContext.Slug.Length - 1; i++) {
                    if (slugContext.Slug[i] == '-') {
                        int j = 0;
                        while (i + j + 1 < slugContext.Slug.Length && slugContext.Slug[i + j + 1] == '-') {
                            j++;
                        }
                        if (j > 0) {
                            slugContext.Slug = slugContext.Slug.Remove(i + 1, j);
                        }
                    }
                }

                if (slugContext.Slug.Length > 1000) {
                    slugContext.Slug = slugContext.Slug.Substring(0, 1000);
                }

                slugContext.Slug = slugContext.Slug.Trim('-', '_', '.');
            }

            _slugEventHandler.FilledSlugFromTitle(slugContext);

            return slugContext.Slug;
        }

        public string Slugify(string text) {
            return Slugify(new FillSlugContext(null, text));
        }
    }
}
