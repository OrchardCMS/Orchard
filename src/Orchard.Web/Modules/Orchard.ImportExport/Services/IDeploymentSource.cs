using System.Collections.Generic;
using System.IO;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IDeploymentSource : IDependency {
        string GetDeploymentFile(RecipeRequest request);

        IList<DeploymentContentType> GetContentTypes();
        IList<DeploymentQuery> GetQueries();
    }

    public interface IDeploymentSourceProvider : IDependency {
        DeploymentSourceMatch Match(IContent sourceConfiguration);
    }

    public class DeploymentSourceMatch {
        public int Priority { get; set; }
        public IDeploymentSource DeploymentSource { get; set; }
    }
}