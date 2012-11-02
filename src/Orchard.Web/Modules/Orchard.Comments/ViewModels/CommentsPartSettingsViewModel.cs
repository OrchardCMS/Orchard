using System.Collections.Generic;
using System.Linq;
using Orchard.Comments.Settings;
using Orchard.Services;

namespace Orchard.Comments.ViewModels {
    public class CommentsPartSettingsViewModel {
        public CommentsPartSettingsViewModel() {
            HtmlFilters = Enumerable.Empty<IHtmlFilter>();
        }

        public CommentsPartSettings Settings { get; set; }
        public IEnumerable<IHtmlFilter> HtmlFilters { get; set; }
    }
}