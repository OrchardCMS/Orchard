using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Events;

namespace Orchard.PackageManager.Events {
    public interface IExtensionDisplayEventHandler : IEventHandler {
        /// <summary>
        /// Called before an extension is displayed
        /// </summary>
        IEnumerable<string> Displaying(ExtensionDescriptor extensionDescriptor);
    }
}