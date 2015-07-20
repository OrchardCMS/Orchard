using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface ISetupService : IDependency {
        string Setup(SetupContext context);
    }
}