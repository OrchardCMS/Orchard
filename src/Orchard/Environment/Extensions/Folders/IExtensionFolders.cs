using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Yaml.Grammar;

namespace Orchard.Environment.Extensions.Folders {
    public interface IExtensionFolders {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }


}