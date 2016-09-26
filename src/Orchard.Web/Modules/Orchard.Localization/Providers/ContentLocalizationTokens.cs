using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Localization.Services;
using Orchard.Tokens;

namespace Orchard.Localization.Providers {
    public class ContentLocalizationTokens : ITokenProvider {
        private readonly ILocalizationService _localizationService;

        public ContentLocalizationTokens(ILocalizationService localizationService) {
            _localizationService = localizationService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content", T("Content Items"), T("Content Items"))
                .Token("Culture", T("Culture"), T("The culture of the content item"), "Culture")
                ;


            context.For("Culture", T("Culture"), T("Tokens for culture"))
                .Token("Name", T("Name"), T("Gets the culture name in the format languagecode2-country/regioncode2."), "Text")
                .Token("TwoLetterISOLanguageName", T("Two Letter ISO Language Name"), T("Gets the ISO 639-1 two-letter code for the language of the current CultureInfo."), "Text");
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                .Token("Culture", x => _localizationService.GetContentCulture(x))
                .Chain("Culture", "Culture", x => CultureInfo.GetCultureInfo(_localizationService.GetContentCulture(x)))
                ;

            context.For<CultureInfo>("Culture")
               .Token("Name", info => info.Name)
               .Token("TwoLetterISOLanguageName", info => info.TwoLetterISOLanguageName)
            ;
        }
    }
}