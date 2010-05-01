namespace Orchard.Setup.Services {
    public interface ISetupService : IDependency {
        void Setup(SetupContext context);
    }
}