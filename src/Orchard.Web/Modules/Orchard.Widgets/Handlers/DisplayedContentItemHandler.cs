using System.Collections.ObjectModel;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Widgets.Handlers {
    /// <summary>
    /// Saves references to content items which have been displayed during a request
    /// </summary>
    public class DisplayedContentItemDetailHandler : ContentHandler, IDisplayedContentItemHandler {
        private readonly Collection<string> _contentTypes = new Collection<string>();

        protected override void BuildDisplayShape(BuildDisplayContext context) {
            if (context.DisplayType == "Detail") {
                _contentTypes.Add(context.ContentItem.ContentType);
            }
        }

        public bool IsDisplayed(string contentType) {
            return _contentTypes.Contains(contentType);
        }
    }

    public interface IDisplayedContentItemHandler: IDependency {
        bool IsDisplayed(string contentType);
    }
}