using System.Globalization;

namespace Orchard.Translations.Models {
    public interface ITranslationStatistic {
        CultureInfo Culture { get; set; }
        int Translated { get; set; }
    }
}