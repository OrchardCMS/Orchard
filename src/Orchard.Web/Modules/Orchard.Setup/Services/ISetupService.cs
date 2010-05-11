using Orchard.Environment.Configuration;

namespace Orchard.Setup.Services {
    public interface ISetupService : IDependency {
        ShellSettings Prime();
        void Setup(SetupContext context);
    }
}