using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Localization.Models;

namespace Orchard.Core.Localization.ViewModels {
    public class ContentLocalizationsViewModel {
        public ContentLocalizationsViewModel(IContent part) {
            Id = part.ContentItem.Id;
        }

        public int Id { get; private set; }
        public IEnumerable<Localized> Localizations { get; set; }
    }
}