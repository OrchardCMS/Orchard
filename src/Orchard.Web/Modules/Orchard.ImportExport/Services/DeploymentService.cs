using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Services;

namespace Orchard.ImportExport.Services {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentService : IDeploymentService {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly Lazy<IEnumerable<IDeploymentSourceProvider>> _deploymentSourceProviders;
        private readonly Lazy<IEnumerable<IDeploymentTargetProvider>> _deploymentTargetProviders;
        private readonly IClock _clock;

        public DeploymentService(
            IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            Lazy<IEnumerable<IDeploymentSourceProvider>> deploymentSourceProviders,
            Lazy<IEnumerable<IDeploymentTargetProvider>> deploymentTargetProviders,
            IClock clock
            ) {
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _deploymentSourceProviders = deploymentSourceProviders;
            _deploymentTargetProviders = deploymentTargetProviders;
            _clock = clock;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; private set; }

        public string DeploymentStoragePath {
            get { return "Deployments"; }
        }

        public IDeploymentSource GetDeploymentSource(IContent configuration) {
            var bestSourceProviderMatch = _deploymentSourceProviders.Value
                .Select(provider => provider.Match(configuration))
                .Where(match => match != null && match.DeploymentSource != null)
                .OrderByDescending(match => match.Priority)
                .FirstOrDefault();

            return bestSourceProviderMatch != null
                ? bestSourceProviderMatch.DeploymentSource
                : null;
        }

        public IDeploymentTarget GetDeploymentTarget(IContent configuration) {
            var bestTargetProviderMatch = _deploymentTargetProviders.Value
                .Select(provider => provider.Match(configuration))
                .Where(match => match != null && match.DeploymentTarget != null)
                .OrderByDescending(match => match.Priority)
                .FirstOrDefault();

            return bestTargetProviderMatch != null
                ? bestTargetProviderMatch.DeploymentTarget
                : null;
        }

        public List<IContent> GetDeploymentSourceConfigurations() {
            return _orchardServices.ContentManager
                .Query(
                    GetDeploymentConfigurationContentTypes()
                        .Select(c => c.Name)
                        .ToArray())
                .List<IContent>()
                .Where(config =>
                    GetDeploymentSource(config) != null)
                .ToList();
        }

        public List<IContent> GetDeploymentTargetConfigurations() {
            return _orchardServices.ContentManager
                .Query(
                    GetDeploymentConfigurationContentTypes()
                        .Select(c => c.Name)
                        .ToArray())
                .List<IContent>()
                .Where(config =>
                    GetDeploymentTarget(config) != null)
                .ToList();
        }

        public List<ContentTypeDefinition> GetDeploymentConfigurationContentTypes() {
            return _orchardServices.ContentManager.GetContentTypeDefinitions()
                .Where(contentTypeDefinition =>
                    contentTypeDefinition.Settings
                        .ContainsKey("Stereotype")
                    && contentTypeDefinition.Settings["Stereotype"]
                        .Split(',')
                        .Select(s => s.Trim())
                        .Contains("DeploymentConfiguration"))
                .ToList();
        }

        public List<DeployableItemTargetPart> GetItemsPendingDeployment(IContent deploymentTarget) {
            return _orchardServices.ContentManager
                .Query<DeployableItemTargetPart, DeployableItemTargetPartRecord>()
                .Where(c =>
                    c.DeploymentTargetId == deploymentTarget.Id
                    && c.DeploymentStatus == DeploymentStatus.Queued.ToString())
                .List()
                .ToList();
        }

        public DeployableItemTargetPart GetDeploymentItemTarget(IContent deployableContent, IContent targetConfiguration, bool createIfNotFound = true) {
            var itemTarget = _orchardServices.ContentManager
                .Query<DeployableItemTargetPart, DeployableItemTargetPartRecord>()
                .Where(c => 
                    c.DeployableContentId == deployableContent.Id
                    && c.DeploymentTargetId == targetConfiguration.Id)
                .Slice(0, 2)
                .SingleOrDefault();

            if (itemTarget != null || !createIfNotFound) {
                return itemTarget;
            }
            itemTarget = _orchardServices.ContentManager.Create<DeployableItemTargetPart>("DeployableItemTarget");
            itemTarget.DeployableContent = deployableContent;
            itemTarget.DeploymentTarget = targetConfiguration;

            return itemTarget;
        }

        public void DeployContent(DeployableItemTargetPart deployableContent) {
            var targetConfigs = GetDeploymentTargetConfigurations();
            foreach (var config in targetConfigs) {
                DeployContentToTarget(deployableContent, config);
            }
        }

        public void DeployContentToTarget(IContent content, IContent targetConfiguration, bool deployAsDraft = false) {
            var deploymentTarget = GetDeploymentTarget(targetConfiguration);
            if (deploymentTarget == null) return;
            
            var itemTarget = GetDeploymentItemTarget(content, targetConfiguration);
            try {
                deploymentTarget.PushContent(content, deployAsDraft);
                itemTarget.DeploymentStatus = DeploymentStatus.Successful;
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error deploying content item {0}", content.Id);
                itemTarget.DeploymentStatus = DeploymentStatus.Failed;
            }

            itemTarget.DeployedUtc = _clock.UtcNow;
        }

        public List<ContentItem> GetContentForExport(RecipeRequest request, int? queuedToTargetId = null) {
            var contentItems = new List<ContentItem>();

            //ensure date is specified as Utc
            var deployChangesAfterUtc = request.DeployChangesAfterUtc.HasValue ? new DateTime(request.DeployChangesAfterUtc.Value.Ticks, DateTimeKind.Utc) : (DateTime?) null;

            if (request.ContentIdentities != null && request.ContentIdentities.Any()) {
                return request.ContentIdentities
                    .Select(c => _orchardServices.ContentManager
                        .ResolveIdentity(new ContentIdentity(c)))
                    .Where(c => c != null).ToList();
            }

            if (!string.IsNullOrEmpty(request.QueryIdentity)) {
                var queryItem = _orchardServices.ContentManager
                    .ResolveIdentity(new ContentIdentity(request.QueryIdentity));
                return _projectionManager.GetContentItems(queryItem.Id).ToList();
            }

            if (request.ContentTypes == null || !request.ContentTypes.Any()) {
                return contentItems;
            }
            var version = request.VersionHistoryOption.HasFlag(VersionHistoryOptions.Draft)
                ? VersionOptions.Draft
                : VersionOptions.Published;
            var query = _orchardServices.ContentManager.HqlQuery()
                .ForType(request.ContentTypes.ToArray())
                .ForVersion(version);

            if (queuedToTargetId.HasValue) {
                var pendingItems = _orchardServices.ContentManager
                    .Query<DeployableItemTargetPart>()
                    .Where<DeployableItemTargetPartRecord>(c =>
                        c.DeploymentStatus == DeploymentStatus.Queued.ToString()
                        && c.DeploymentTargetId == queuedToTargetId.Value)
                    .List()
                    .Select(c => c.DeployableContent.Id)
                    .ToList();

                if (!pendingItems.Any()) {
                    //if there are no queued items, no need to run outer query
                    return contentItems;
                }

                query = query.Where(a => a.ContentItem(), p => p.In("Id", pendingItems));
            }

            if (deployChangesAfterUtc.HasValue) {
                var changeDateField =
                    request.VersionHistoryOption == VersionHistoryOptions.Published
                    ? "PublishedUtc"
                    : "ModifiedUtc";
                query = query.Where(
                    a => a.ContentPartRecord(typeof (CommonPartRecord)),
                    exp => exp.Gt(changeDateField, deployChangesAfterUtc));
            }

            //Order by id so that dependencies are usually imported first
            //Also needed to fix bug with HQL in orchard 1.7 pre-release where query would fail if order not specified
            query = query.OrderBy(a => a.ContentItem(), o => o.Asc("Id"));
            contentItems = query.List().ToList();

            return contentItems;
        }

        public void UpdateDeployableContentStatus(string executionId, DeploymentStatus status) {
            var deployedItemTargets = _orchardServices.ContentManager
                .Query<DeployableItemTargetPart>()
                .Where<DeployableItemTargetPartRecord>(d => d.ExecutionId == executionId)
                .ForVersion(VersionOptions.Latest)
                .List();

            foreach (var item in deployedItemTargets) {
                item.DeploymentStatus = status;
            }
        }
    }
}
