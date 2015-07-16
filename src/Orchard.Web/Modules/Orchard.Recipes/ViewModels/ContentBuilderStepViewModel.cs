using System.Collections.Generic;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.ViewModels {
    public class ContentBuilderStepViewModel {
        public ContentBuilderStepViewModel() {
            ContentTypes = new List<ContentTypeEntry>();
        }

        public IList<ContentTypeEntry> ContentTypes { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }
    }
}