using System.Collections.Generic;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        IEnumerable<RequireSettings> GetRequiredResources(string type);
        IList<ResourceRequiredContext> BuildRequiredResources(string resourceType);
        IList<LinkEntry> GetRegisteredLinks();
        IList<MetaEntry> GetRegisteredMetas();
        IEnumerable<IResourceManifest> ResourceProviders { get; }
        ResourceManifest DynamicResources { get; }
        ResourceDefinition FindResource(RequireSettings settings);
        void NotRequired(string resourceType, string resourceName);
        RequireSettings Require(string resourceType, string resourceName);
        void RegisterLink(LinkEntry link);
        void SetMeta(MetaEntry meta);
        void AppendMeta(MetaEntry meta, string contentSeparator);
    }
}
