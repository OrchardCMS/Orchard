﻿using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders {
    public class ModuleFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly IExtensionHarvester _extensionHarvester;

        public ModuleFolders(IEnumerable<string> paths, IExtensionHarvester extensionHarvester)  {
            _paths = paths;
            _extensionHarvester = extensionHarvester;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(_paths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}