using System.Collections.Generic;
using Orchard.Core.Localization.Models;

namespace Orchard.Core.Localization.ViewModels {
    public class ContentLocalizationsViewModel {
        public ContentLocalizationsViewModel(Localized part) {
            MasterId = part.MasterContentItem != null
                ? part.MasterContentItem.ContentItem.Id
                : part.Id;
        }

        public int? MasterId { get; private set; }
        public IEnumerable<Localized> Localizations { get; set; }
    }
}