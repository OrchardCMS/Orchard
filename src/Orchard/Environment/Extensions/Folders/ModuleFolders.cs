using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders {
    public class ModuleFolders : IExtensionFolders {
        public IEnumerable<string> Paths { get; private set; }
        private readonly IExtensionHarvester _extensionHarvester;

        public ModuleFolders(IEnumerable<string> paths, IExtensionHarvester extensionHarvester)  {
            Paths = paths;
            _extensionHarvester = extensionHarvester;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(Paths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}