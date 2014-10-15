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
        private readonly bool fOptimizeForMemoryUsage = false;

        public TransliterationService(IRepository<TransliterationSpecificationRecord> transliterationRepository) {
            _transliterationRepository = transliterationRepository;
        }

        public string Convert(string value, string cultureFrom) {
            var transliterationSpecification = _transliterationRepository.Get(x => x.CultureFrom == cultureFrom);
            if (transliterationSpecification == null) return value;

            var specification = GetSpecification(transliterationSpecification);

            Transliterator transliterator = Transliterator.FromSpecification(specification, fOptimizeForMemoryUsage);
            
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

        public void Create(string cultureFrom, string cultureTo, string rules) {
            _transliterationRepository.Create(new TransliterationSpecificationRecord {
                CultureFrom = cultureFrom, 
                CultureTo = cultureTo,
                Rules = rules
            });
        }

        public void Update(int id, string cultureFrom, string cultureTo, string rules) {
            var record = _transliterationRepository.Get(id);
            record.CultureFrom = cultureFrom;
            record.CultureTo = cultureTo;
            record.Rules = rules;
            _transliterationRepository.Update(record);
        }

        public void Remove(int id) {
            _transliterationRepository.Delete(_transliterationRepository.Get(id));
        }

        public TransliterationSpecificationRecord Get(int id) {
            return _transliterationRepository.Get(id);
        }

        private TransliteratorSpecification GetSpecification(TransliterationSpecificationRecord record) {
            using (TextReader specificationReader = new StringReader(record.Rules)) {
                return TransliteratorSpecification.FromSpecificationFile(specificationReader, fOnlyMetadata);
            }
        }
    }
}