using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Helpers;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Utility;
using Orchard.Utility.Extensions;

namespace Orchard.Environment.Extensions {
    public class ExtensionManager : IExtensionManager {
        private readonly IEnumerable<IExtensionFolders> _folders;
        private readonly IEnumerable<IExtensionLoader> _loaders;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ExtensionManager(IEnumerable<IExtensionFolders> folders, IEnumerable<IExtensionLoader> loaders) {
            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order);
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        // This method does not load extension types, simply parses extension manifests from 
        // the filesystem. 
        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _folders.SelectMany(folder => folder.AvailableExtensions());
        }

        public IEnumerable<FeatureDescriptor> AvailableFeatures() {
            var featureDescriptors = AvailableExtensions().SelectMany(ext => ext.Features);
            var featureDescriptorsOrdered = featureDescriptors.OrderByDependencies(HasDependency);
            return featureDescriptorsOrdered.ToReadOnlyCollection();
        }

        /// <summary>
        /// Returns true if the item has an explicit or implicit dependency on the subject
        /// </summary>
        /// <param name="item"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        internal static bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject) {
            if (item.Extension.ExtensionType == "Theme") {
                // Themes implicitly depend on modules to ensure build and override ordering
                if (subject.Extension.ExtensionType == "Module") {
                    return true;
                }
                if (subject.Extension.ExtensionType == "Theme") {
                    // theme depends on another if it is its base theme
                    return item.Extension.BaseTheme == subject.Name;
                }
            }

            // Return based on explicit dependencies
            return item.Dependencies != null &&
                   item.Dependencies.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x, subject.Name));
        }

        private IEnumerable<ExtensionEntry> LoadedExtensions() {
            foreach ( var descriptor in AvailableExtensions() ) {
                ExtensionEntry entry = null;
                try {
                    entry = BuildEntry(descriptor);
                }
                catch (HttpCompileException ex) {
                    Logger.Warning(ex, "Unable to load extension {0}", descriptor.Name);
                }
                if (entry != null)
                    yield return entry;
            }
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            return featureDescriptors
                .Select(LoadFeature)
                .ToArray();
        }

        private Feature LoadFeature(FeatureDescriptor featureDescriptor) {
            var featureName = featureDescriptor.Name;

            string extensionName = GetExtensionForFeature(featureName);
            if (extensionName == null)
                throw new ArgumentException(T("Feature {0} was not found in any of the installed extensions", featureName).ToString());

            var extension = LoadedExtensions().Where(x => x.Descriptor.Name == extensionName).FirstOrDefault();
            if (extension == null)
                throw new InvalidOperationException(T("Extension {0} is not active", extensionName).ToString());

            var extensionTypes = extension.ExportedTypes.Where(t => t.IsClass && !t.IsAbstract);
            var featureTypes = new List<Type>();

            foreach (var type in extensionTypes) {
                string sourceFeature = GetSourceFeatureNameForType(type, extensionName);
                if (String.Equals(sourceFeature, featureName, StringComparison.OrdinalIgnoreCase)) {
                    featureTypes.Add(type);
                }
            }

            return new Feature {
                Descriptor = featureDescriptor,
                ExportedTypes = featureTypes
            };
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionName) {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetCustomAttributes(typeof(OrchardFeatureAttribute), false)) {
                return featureAttribute.FeatureName;
            }
            return extensionName;
        }

        private string GetExtensionForFeature(string featureName) {
            foreach (var extensionDescriptor in AvailableExtensions()) {
                if (String.Equals(extensionDescriptor.Name, featureName, StringComparison.OrdinalIgnoreCase)) {
                    return extensionDescriptor.Name;
                }
                foreach (var feature in extensionDescriptor.Features) {
                    if (String.Equals(feature.Name, featureName, StringComparison.OrdinalIgnoreCase)) {
                        return extensionDescriptor.Name;
                    }
                }
            }
            return null;
        }

        public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
            if (String.IsNullOrEmpty(extensionType)) {
                throw new ArgumentException(T("extensionType was null or empty").ToString());
            }
            string targetFolder;
            if (String.Equals(extensionType, "Theme", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Themes");
            }
            else if (String.Equals(extensionType, "Module", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Modules");
            }
            else {
                throw new ArgumentException(T("extensionType was not recognized").ToString());
            }
            int postedFileLength = extensionBundle.ContentLength;
            Stream postedFileStream = extensionBundle.InputStream;
            byte[] postedFileData = new byte[postedFileLength];
            postedFileStream.Read(postedFileData, 0, postedFileLength);

            using (var memoryStream = new MemoryStream(postedFileData)) {
                var fileInflater = new ZipInputStream(memoryStream);
                ZipEntry entry;
                while ((entry = fileInflater.GetNextEntry()) != null) {
                    string directoryName = Path.GetDirectoryName(entry.Name);
                    if (!Directory.Exists(Path.Combine(targetFolder, directoryName))) {
                        Directory.CreateDirectory(Path.Combine(targetFolder, directoryName));
                    }

                    if (!entry.IsDirectory && entry.Name.Length > 0) {
                        var len = Convert.ToInt32(entry.Size);
                        var extractedBytes = new byte[len];
                        fileInflater.Read(extractedBytes, 0, len);
                        File.WriteAllBytes(Path.Combine(targetFolder, entry.Name), extractedBytes);
                    }
                }
            }
        }

        public void UninstallExtension(string extensionType, string extensionName) {
            if (String.IsNullOrEmpty(extensionType)) {
                throw new ArgumentException(T("extensionType was null or empty").ToString());
            }
            string targetFolder;
            if (String.Equals(extensionType, "Theme", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Themes");
            }
            else if (String.Equals(extensionType, "Module", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Modules");
            }
            else {
                throw new ArgumentException(T("extensionType was not recognized").ToString());
            }
            targetFolder = Path.Combine(targetFolder, extensionName);
            if (!Directory.Exists(targetFolder)) {
                throw new ArgumentException(T("extension was not found").ToString());
            }
            Directory.Delete(targetFolder, true);
        }

        private ExtensionEntry BuildEntry(ExtensionDescriptor descriptor) {
            foreach (var loader in _loaders) {
                ExtensionEntry entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }

            Logger.Warning("No suitable loader found for extension \"{0}\"", descriptor.Name);
            return null;
        }
    }
}