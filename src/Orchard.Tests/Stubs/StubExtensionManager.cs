using System;
using System.Collections.Generic;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.Stubs {
    public class StubExtensionManager : IExtensionManager {
        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            throw new NotSupportedException();
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures() {
            throw new NotSupportedException();
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            throw new NotSupportedException();
        }

        public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
            throw new NotSupportedException();
        }

        public void UninstallExtension(string extensionType, string extensionName) {
            throw new NotSupportedException();
        }
    }
}
