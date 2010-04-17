using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    public interface IExtensionLoader {
        int Order { get; }
        ExtensionEntry Load(ExtensionDescriptor descriptor);
    }
}