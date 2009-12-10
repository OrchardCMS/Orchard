namespace Orchard.Extensions.Loaders {
    public interface IExtensionLoader {
        int Order { get; }
        ExtensionEntry Load(ExtensionDescriptor descriptor);
    }
}