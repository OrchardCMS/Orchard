using System.IO;
using System.NaturalLanguage.Tools;
using System.Text;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.Transliteration")]
    public class TransliterationServices : ITransliterationServices {
        private readonly IRepository<TransliterationSpecificationRecord> _transliterationRepository;
        private readonly bool fOnlyMetadata = false;

        public TransliterationServices(IRepository<TransliterationSpecificationRecord> transliterationRepository) {
            _transliterationRepository = transliterationRepository;
        }

        public string Convert(string value, int transliterationSpecificationId) {
            var transliterationSpecification = _transliterationRepository.Get(transliterationSpecificationId);
            if (transliterationSpecification == null) return value;

            var specification = GetSpecification(transliterationSpecification);

            Transliterator transliterator = Transliterator.FromSpecification(specification);

            // TODO : Return the contents of this
            var transliteratorRuleTraceList = new TransliteratorRuleTraceList();

            return transliterator.Transliterate(
                value, 
                new StringBuilder(value.Length * 2),
                transliteratorRuleTraceList);
        }

        private TransliteratorSpecification GetSpecification(TransliterationSpecificationRecord record) {
            using (TextReader specificationReader = new StringReader(record.Rules)) {
                return TransliteratorSpecification.FromSpecificationFile(specificationReader, fOnlyMetadata);
            }
        }
    }
}