using System;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.UI.Resources;

namespace Orchard.DisplayManagement.Descriptors.ResourceBindingStrategy {
    /// <summary>
    /// Discovers .js files and turns them into Script__<filename> shapes.
    /// </summary>
    public class ScriptBindingStrategy : StaticFileBindingStrategy, IShapeTableProvider {
        public ScriptBindingStrategy(IExtensionManager extensionManager, ShellDescriptor shellDescriptor, IVirtualPathProvider virtualPathProvider)
            : base(extensionManager, shellDescriptor, virtualPathProvider) {
        }

        public override string GetFileExtension() {
            return ".js";
        }

        public override string GetFolder() {
            return "Scripts";
        }

        public override string GetShapePrefix() {
            return "Script__";
        }

        public virtual string GetShapeName() {
            return "Script";
        }

        public override void Discover(ShapeTableBuilder builder) {
            base.Discover(builder);

            builder.Describe("Script")
                .OnDisplaying(displaying => {
                    var resourceShape = displaying.Shape;
                    var url = (string)resourceShape.Url;
                    var settings = (RequireSettings)resourceShape.Settings;
                    if (settings.LoadAsync) {
                        displaying.ShapeMetadata.Alternates.Clear();
                        displaying.ShapeMetadata.Type = GetShapeName();

                        var fileName = GetAlternateShapeNameFromFileName(url);
                        if (!String.IsNullOrEmpty(fileName)) {
                            resourceShape.Metadata.Alternates.Add(GetShapePrefix() + fileName);
                        }
                    }
                });
        }
    }
}