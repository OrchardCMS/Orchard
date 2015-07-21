using Orchard.ContentManagement;

namespace Orchard.ImportExport.Services {
    public interface IDeploymentTarget : IDependency {
        void PushDeploymentFile(string deploymentExecutionId, string deploymentFilePath);
        void PushRecipe(string deploymentExecutionId, string recipeText);
        bool? GetRecipeDeploymentStatus(string deploymentExecutionId);
        void PushContent(IContent content, bool deployAsDraft = false);
    }

    public interface IDeploymentTargetProvider : IDependency {
        DeploymentTargetMatch Match(IContent targetConfiguration);
    }

    public class DeploymentTargetMatch {
        public int Priority { get; set; }
        public IDeploymentTarget DeploymentTarget { get; set; }
    }
}