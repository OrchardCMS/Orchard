using System.Collections.Generic;

namespace Orchard.UI.Resources {
    public interface IResourceManifest : ISingletonDependency {
        ResourceDefinition DefineResource(string resourceType, string resourceName);
        string Name { get; }
        string BasePath { get; }
        IDictionary<string, ResourceDefinition> GetResources(string resourceType);
    }



    //public class DefaultResMgr {
    //    public DefaultResMgr(IEnumerable<IResourceDefinitionProvider> providers) {

    //    }
    //}

    //public interface IResourceDefinitionProvider {
    //    void GetResources(BlahBuildsinkTargetSomethingToto def);
    //}
}
