using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public interface IDeploymentService : IDependency {
        string DeploymentStoragePath { get; }
        IDeploymentSource GetDeploymentSource(IContent sourceConfiguration);
        IDeploymentTarget GetDeploymentTarget(IContent targetConfiguration);
        List<IContent> GetDeploymentSourceConfigurations();
        List<IContent> GetDeploymentTargetConfigurations();
        List<ContentTypeDefinition> GetDeploymentConfigurationContentTypes();

        List<DeployableItemTargetPart> GetItemsPendingDeployment(IContent targetConfiguration);
        DeployableItemTargetPart GetDeploymentItemTarget(IContent deployableContent, IContent targetConfiguration, bool createIfNotFound = true);
        void DeployContent(DeployableItemTargetPart deployableContent);
        void DeployContentToTarget(IContent content, IContent targetConfiguration, bool deployAsDraft = false);

        List<ContentItem> GetContentForExport(RecipeRequest request, int? queuedToTargetId = null);
        void UpdateDeployableContentStatus(string executionId, DeploymentStatus status);
    }
}