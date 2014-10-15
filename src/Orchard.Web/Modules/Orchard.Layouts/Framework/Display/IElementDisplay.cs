using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Display {
    public interface IElementDisplay : IDependency {
        dynamic DisplayElement(IElement element, IContent content, string displayType = null, IUpdateModel updater = null, string renderEventName = null, string renderEventArgs = null);
        dynamic DisplayElements(IEnumerable<IElement> elements, IContent content, string displayType = null, IUpdateModel updater = null, string renderEventName = null, string renderEventArgs = null);
    }
}