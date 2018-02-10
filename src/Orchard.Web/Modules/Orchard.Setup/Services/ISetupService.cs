using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Recipes.Models;

namespace Orchard.Setup.Services {
    public interface ISetupService : IDependency {
        ShellSettings Prime();
        IEnumerable<Recipe> Recipes();
        string Setup(SetupContext context);
    }
}