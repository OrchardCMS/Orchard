using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Orchard.Environment;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.UI.Resources;
using Orchard.Utility.Extensions;

namespace Orchard.DisplayManagement.Descriptors.ResourceBindingStrategy {
    // discovers static files and turns them into shapes.
    public abstract class StaticFileBindingStrategy {
        private static readonly char[] _queryStringChars = new[] { '?' };
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private static readonly char[] UnsafeCharList = "/:?#[]@!&'()*+,;=\r\n\t\f\" <>.-_".ToCharArray();
        private readonly Work<WorkContext> _workContext;
        private readonly IResourceFileHashProvider _resourceFileHashProvider;

        protected StaticFileBindingStrategy(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IVirtualPathProvider virtualPathProvider,
            Work<WorkContext> workContext,
            IResourceFileHashProvider resourceFileHashProvider) {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _virtualPathProvider = virtualPathProvider;
            _workContext = workContext;
            _resourceFileHashProvider = resourceFileHashProvider;
        }

        public abstract string GetFileExtension();
        public abstract string GetFolder();
        public abstract string GetShapePrefix();

        private static string SafeName(string name) {
            if (string.IsNullOrWhiteSpace(name))
                return String.Empty;

            return name.Strip(UnsafeCharList).ToLowerInvariant();
        }

        public static string GetAlternateShapeNameFromFileName(string fileName) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }
            string shapeName;
            if (Uri.IsWellFormedUriString(fileName, UriKind.Absolute)
                || (fileName.StartsWith("//", StringComparison.InvariantCulture)
                && Uri.IsWellFormedUriString("http:" + fileName, UriKind.Absolute))) {
                if (fileName.StartsWith("//", StringComparison.InvariantCulture)) {
                    fileName = "http:" + fileName;
                }
                var uri = new Uri(fileName);
                shapeName = uri.Authority + "$" + uri.AbsolutePath + "$" + uri.Query;
            }
            else {
                shapeName = Path.GetFileNameWithoutExtension(fileName);
            }
            return SafeName(shapeName);
        }

        private static IEnumerable<ExtensionDescriptor> Once(IEnumerable<FeatureDescriptor> featureDescriptors) {
            var once = new ConcurrentDictionary<string, object>();
            return featureDescriptors.Select(fd => fd.Extension).Where(ed => once.TryAdd(ed.Id, null)).ToList();
        }

        public void Discover(ShapeTableBuilder builder) {
            var availableFeatures = _extensionManager.AvailableFeatures();
            var activeFeatures = availableFeatures.Where(FeatureIsEnabled);
            var activeExtensions = Once(activeFeatures);

            var hits = activeExtensions.SelectMany(extensionDescriptor => {
                var basePath = Path.Combine(extensionDescriptor.Location, extensionDescriptor.Id).Replace(Path.DirectorySeparatorChar, '/');
                var virtualPath = Path.Combine(basePath, GetFolder()).Replace(Path.DirectorySeparatorChar, '/');
                var shapes = _virtualPathProvider.ListFiles(virtualPath)
                    .Select(Path.GetFileName)
                    .Where(fileName => string.Equals(Path.GetExtension(fileName), GetFileExtension(), StringComparison.OrdinalIgnoreCase))
                    .Select(cssFileName => new {
                        fileName = Path.GetFileNameWithoutExtension(cssFileName),
                        fileVirtualPath = Path.Combine(virtualPath, cssFileName).Replace(Path.DirectorySeparatorChar, '/'),
                        shapeType = GetShapePrefix() + GetAlternateShapeNameFromFileName(cssFileName),
                        extensionDescriptor
                    });
                return shapes;
            });

            foreach (var iter in hits) {
                var hit = iter;
                var featureDescriptors = hit.extensionDescriptor.Features.Where(fd => fd.Id == hit.extensionDescriptor.Id);
                foreach (var featureDescriptor in featureDescriptors) {
                    builder.Describe(iter.shapeType)
                        .From(new Feature { Descriptor = featureDescriptor })
                        .BoundAs(
                            hit.fileVirtualPath,
                            shapeDescriptor => displayContext => {
                                var shape = ((dynamic)displayContext.Value);
                                var output = displayContext.ViewContext.Writer;
                                ResourceDefinition resource = shape.Resource;
                                var url = GetResourceUrl(shape.Url, AddHash(hit.fileVirtualPath));
                                string condition = shape.Condition;
                                Dictionary<string, string> attributes = shape.TagAttributes;
                                ResourceManager.WriteResource(output, resource, url, condition, attributes);
                                return null;
                            });
                }
            }
        }


        private string AddHash(string url) {
            var site = _workContext.Value.CurrentSite;

            // Adds the hash of the static resources if neded
            if (site.UseFileHash) {
                var physicalPath = GetPhysicalPath(url);
                if (!String.IsNullOrEmpty(physicalPath) && File.Exists(physicalPath)) {
                    return AddQueryStringValue(url, "fileHash", _resourceFileHashProvider.GetResourceFileHash(physicalPath));
                }
            }
            return url;
        }

        private string GetPhysicalPath(string url) {
            if (!String.IsNullOrEmpty(url) && !Uri.IsWellFormedUriString(url, UriKind.Absolute) && !url.StartsWith("//")) {
                if (VirtualPathUtility.IsAbsolute(url) || VirtualPathUtility.IsAppRelative(url)) {
                    return HostingEnvironment.MapPath(url.Split(_queryStringChars)[0]);
                }
            }
            return null;
        }

        private string AddQueryStringValue(string url, string name, string value) {
            if (String.IsNullOrEmpty(url)) {
                return null;
            }
            var encodedValue = HttpUtility.UrlEncode(value);
            if (url.Contains("?")) {
                if (url.EndsWith("&")) {
                    return String.Format("{0}{1}={2}", url, name, encodedValue);
                }
                else {
                    return String.Format("{0}&{1}={2}", url, name, encodedValue);
                }
            }
            else {
                return String.Format("{0}?{1}={2}", url, name, encodedValue);
            }
        }

        private string GetResourceUrl(string shapeUrl, string fileVirtualPath) {
            if (string.IsNullOrEmpty(shapeUrl)) return fileVirtualPath;

            return GetPathFromRelativeUrl(shapeUrl).Equals(GetPathFromRelativeUrl(fileVirtualPath), StringComparison.InvariantCultureIgnoreCase) ?
                shapeUrl : fileVirtualPath;
        }

        private string GetPathFromRelativeUrl(string url) {
            // normalize urls that could be like ~/ or /OrchardLocal/ or /OrchardLocal/tenant-prefix
            // driving them to a ~/ format
            var appRelativeUrl = System.Web.VirtualPathUtility.ToAppRelative(url);
            var path = appRelativeUrl.TrimStart('~');
            var indexOfQueryString = path.IndexOf('?');

            return indexOfQueryString >= 0 ? path.Substring(0, indexOfQueryString) : path;
        }

        private bool FeatureIsEnabled(FeatureDescriptor fd) {
            return (DefaultExtensionTypes.IsTheme(fd.Extension.ExtensionType) && (fd.Id == "TheAdmin" || fd.Id == "SafeMode")) ||
                _shellDescriptor.Features.Any(sf => sf.Name == fd.Id);
        }
    }

    // discovers .css files and turns them into Style__<filename> shapes.
    public class StylesheetBindingStrategy : StaticFileBindingStrategy, IShapeTableProvider {
        public StylesheetBindingStrategy(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IVirtualPathProvider virtualPathProvider,
            Work<WorkContext> workContext,
            IResourceFileHashProvider resourceFileHashProvider) : base(
                extensionManager,
                shellDescriptor,
                virtualPathProvider,
                workContext,
                resourceFileHashProvider) {
        }

        public override string GetFileExtension() {
            return ".css";
        }

        public override string GetFolder() {
            return "Styles";
        }

        public override string GetShapePrefix() {
            return "Style__";
        }
    }
}
