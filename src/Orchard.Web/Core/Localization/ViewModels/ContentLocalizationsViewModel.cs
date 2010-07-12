using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Core.Localization.ViewModels {
    public class ContentLocalizationsViewModel {
        public ContentLocalizationsViewModel(IContent part) {
            Id = part.ContentItem.Id;
        }

        public int Id { get; private set; }
        public bool CanLocalize { get; set; }
        public IEnumerable<IContent> Localizations { get; set; }
    }
}