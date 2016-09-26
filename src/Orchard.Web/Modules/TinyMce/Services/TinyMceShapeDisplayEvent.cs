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
            IWorkContextAccessor workContextAccessor,
            ISignals signals) {
            _signals = signals;
            _cacheManager = cacheManager;
            _virtualPathProvider = virtualPathProvider;
            _workContext = workContextAccessor.GetContext();
        }

        public override void Displaying(ShapeDisplayingContext context) {
            if (String.CompareOrdinal(context.ShapeMetadata.Type, "Body_Editor") != 0) {
                return;
            }

            if (!String.Equals(context.Shape.EditorFlavor, "html", StringComparison.InvariantCultureIgnoreCase)) {
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

                var directoryName = _virtualPathProvider.GetDirectoryName("~/modules/tinymce/scripts/langs/");

                var languageFiles = _virtualPathProvider
                    .ListFiles(directoryName)
                    .Select(file => file.Replace(directoryName + "/", ""))
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