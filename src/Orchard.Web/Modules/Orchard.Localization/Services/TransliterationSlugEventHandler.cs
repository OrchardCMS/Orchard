using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.Transliteration")]
    public class TransliterationSlugEventHandler : ISlugEventHandler {
        private readonly ITransliterationService _transliterationService;

        public TransliterationSlugEventHandler(ITransliterationService transliterationService) {
            _transliterationService = transliterationService;
        }

        public void FillingSlugFromTitle(FillSlugContext context) {
            var localizationAspect = context.Content.As<ILocalizableAspect>();
            if (localizationAspect == null) return;

            context.Title = _transliterationService.Convert(context.Title, localizationAspect.Culture);
            context.Adjusted = true;
        }

        public void FilledSlugFromTitle(FillSlugContext context) {
        }
    }
}