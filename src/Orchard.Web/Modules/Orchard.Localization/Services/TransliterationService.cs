using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.NaturalLanguage.Tools;
using System.Text;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.Transliteration")]
    public class TransliterationService : ITransliterationService {
        private readonly IRepository<TransliterationSpecificationRecord> _transliterationRepository;
        private readonly bool fOnlyMetadata = false;

        public TransliterationService(IRepository<TransliterationSpecificationRecord> transliterationRepository) {
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

        public IEnumerable<TransliterationSpecificationRecord> GetSpecifications() {
            return _transliterationRepository.Table.ToList();
        }

        private TransliteratorSpecification GetSpecification(TransliterationSpecificationRecord record) {
            using (TextReader specificationReader = new StringReader(record.Rules)) {
                return TransliteratorSpecification.FromSpecificationFile(specificationReader, fOnlyMetadata);
            }
        }
    }
}