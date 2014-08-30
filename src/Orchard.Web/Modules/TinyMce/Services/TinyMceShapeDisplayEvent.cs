using System;
using System.Globalization;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.DisplayManagement.Implementation;
using Orchard.FileSystems.VirtualPath;

namespace TinyMce.Services {
    public class TinyMceShapeDisplayEvent : ShapeDisplayEvents {
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly WorkContext _workContext;

        private const string CacheKeyFormat = "tinymce-locales-{0}";
        private const string DefaultLanguage = "en";

        public TinyMceShapeDisplayEvent(
            ICacheManager cacheManager,
            IVirtualPathProvider virtualPathProvider,
            WorkContext workContext,
            ISignals signals) {
            _signals = signals;
            _cacheManager = cacheManager;
            _virtualPathProvider = virtualPathProvider;
            _workContext = workContext;
        }

        public override void Displaying(ShapeDisplayingContext context) {
            if (String.CompareOrdinal(context.ShapeMetadata.Type, "Body_Editor") != 0) {
                return;
            }

            if (String.CompareOrdinal(context.Shape.EditorFlavor, "html") != 0) {
                return;
            }

            context.Shape.Language = GetTinyMceLanguageIdentifier();
        }

        private string GetTinyMceLanguageIdentifier() {
            var currentCulture = CultureInfo.GetCultureInfo(_workContext.CurrentCulture);

            if (currentCulture.Name.Equals(DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return currentCulture.Name;


            return _cacheManager.Get(string.Format(CacheKeyFormat, currentCulture.Name), ctx => {
                ctx.Monitor(_signals.When("culturesChanged"));

                var customLanguage = currentCulture.Name.Replace('-', '_');

                var languageFiles = _virtualPathProvider
                    .ListFiles("~/modules/tinymce/scripts/langs")
                    .ToList();

                if (languageFiles.Any(x => x == string.Format("{0}.js", customLanguage)))
                    return customLanguage;

                if (!DefaultLanguage.Equals(currentCulture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase) &&
                    languageFiles.Any(x => x == string.Format("{0}.js", currentCulture.TwoLetterISOLanguageName))) {
                    return currentCulture.TwoLetterISOLanguageName;
                }

                return DefaultLanguage;
            });
        }
    }
}