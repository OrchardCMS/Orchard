using System.Collections.Generic;
using System.Web.Routing;
using Orchard.Environment.Extensions.Models;
using Orchard.Events;

namespace Orchard.Themes.Events {
    public interface IExtensionDisplayEventHandler : IEventHandler {
        /// <summary>
        /// Called before an extension is displayed
        /// </summary>
        IEnumerable<string> Displaying(ExtensionDescriptor extensionDescriptor, RequestContext requestContext);
    }
}