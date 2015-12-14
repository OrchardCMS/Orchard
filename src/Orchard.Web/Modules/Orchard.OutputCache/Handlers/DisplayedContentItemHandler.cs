using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Orchard.OutputCache.Services;
using Orchard.ContentManagement.Handlers;

namespace Orchard.OutputCache.Handlers {
    /// <summary>
    /// Saves references to content items which have been displayed during a request
    /// </summary>
    public class DisplayedContentItemHandler : ContentHandler, IDisplayedContentItemHandler {
        private readonly Collection<int> _itemIds = new Collection<int>();

        protected override void BuildDisplayShape(BuildDisplayContext context) {
            _itemIds.Add(context.Content.Id);
        }

        public bool IsDisplayed(int id) {
            return _itemIds.Contains(id);
        }

        public IEnumerable<int> GetDisplayed() {
            return _itemIds.Distinct();
        }
    }
}