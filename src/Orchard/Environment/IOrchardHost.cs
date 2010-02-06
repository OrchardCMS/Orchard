using Orchard.Environment.Configuration;

namespace Orchard.Environment {
    public interface IOrchardHost {
        void Initialize();
        void EndRequest();

        IStandaloneEnvironment CreateStandaloneEnvironment(IShellSettings shellSettings);
    }
}
