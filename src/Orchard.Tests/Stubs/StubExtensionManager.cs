using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.Stubs {
    public class StubExtensionManager : IExtensionManager {
        public ExtensionDescriptor GetExtension(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            throw new NotSupportedException();
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures() {
            throw new NotSupportedException();
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            throw new NotSupportedException();
        }
    }
}
