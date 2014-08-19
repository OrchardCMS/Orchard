using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Data;
using Orchard.Localization.Services;
using Orchard.Translations.Models;
using Orchard.Utility.Extensions;

namespace Orchard.Translations.Services {
    public interface ITranslationsManager : IDependency {
        ITranslationStatistic GetStatistic(string cultureName);
        IEnumerable<ITranslatableProject> GetTranslatableProjects(string cultureName);
        void Reset(string cultureName);
        void Delete(string cultureName);
    }

    public class TranslationsManager : ITranslationsManager {
        private readonly IRepository<TranslatableRecord> _translatableRecordRepository;
        private readonly IRepository<TranslatedRecord> _translatedRecordRepository;

        public TranslationsManager(IRepository<TranslatableRecord> translatableRecordRepository,
            IRepository<TranslatedRecord> translatedRecordRepository) {
            _translatableRecordRepository = translatableRecordRepository;
            _translatedRecordRepository = translatedRecordRepository;
        }

        public ITranslationStatistic GetStatistic(string cultureName) {
            var translatedCount = _translatedRecordRepository.Count(x => x.Culture.Culture == cultureName);

            return new TranslationStatistic {
                Culture = CultureInfo.GetCultureInfo(cultureName),
                Translated = translatedCount
            };
        }

        public IEnumerable<ITranslatableProject> GetTranslatableProjects(string cultureName) {
            var translatableRecords = _translatableRecordRepository.Table.ToList();
            var translatedRecords = _translatedRecordRepository.Fetch(x => x.Culture.Culture == cultureName);

            foreach (var translatableRecord in translatableRecords) {
                var translatedRecord = translatedRecords.SingleOrDefault(x => x.Parent.Id == translatableRecord.Id);
                yield return new TranslatableProject {
                    TranslatableRecord = translatableRecord,
                    TranslatedRecord = translatedRecord
                };
            }
        }

        public void Reset(string cultureName) {
            Delete(cultureName);
        }

        public void Delete(string cultureName) {
            var records = _translatedRecordRepository.Fetch(x => x.Culture.Culture == cultureName);
            foreach (var record in records) {
                _translatedRecordRepository.Delete(record);
            }
        }
    }

    public class TranslatableProject : ITranslatableProject {
        public TranslatableRecord TranslatableRecord { get; set; }
        public TranslatedRecord TranslatedRecord { get; set; }
    }

    public class TranslationStatistic : ITranslationStatistic {
        public CultureInfo Culture { get; set; }
        public int Translated { get; set; }
    }
}