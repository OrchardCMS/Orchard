namespace Orchard.Extensions.Loaders {
    public class PrecompiledExtensionLoader : IExtensionLoader {
        public int Order { get { return 3; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            //var assembly = Assembly.Load(descriptor.Name);
            //return new ModuleEntry {
            //    Descriptor = descriptor,
            //    Assembly = assembly,
            //    ExportedTypes = assembly.GetExportedTypes()
            //};
            return null;
        }
    }
}
