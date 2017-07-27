using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Orchard.OutputCache.Services;
using Orchard.ContentManagement.Handlers;

namespace Orchard.OutputCache.Handlers {
    /// <summary>
    /// Creates tags for content items which have been displayed during a request
    /// </summary>
    public class DisplayedContentCacheTagProvider : ContentHandler, ICacheTagProvider {
        private readonly Collection<int> _itemIds = new Collection<int>();

        protected override void BuildDisplayShape(BuildDisplayContext context) {
            _itemIds.Add(context.Content.Id);
        }

        public IEnumerable<string> GetTags() {
            return _itemIds.Distinct().Select(id => id.ToString(CultureInfo.InvariantCulture));
        }
    }
}