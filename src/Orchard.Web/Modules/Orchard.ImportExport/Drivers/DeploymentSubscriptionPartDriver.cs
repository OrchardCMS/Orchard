using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;
using Orchard.Projections.Models;

namespace Orchard.ImportExport.Drivers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentSubscriptionDriver : ContentPartDriver<DeploymentSubscriptionPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IDeploymentService _deploymentService;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public DeploymentSubscriptionDriver(
            IOrchardServices orchardServices,
            IDeploymentService deploymentService
            ) {
            _orchardServices = orchardServices;
            _deploymentService = deploymentService;

            // initializing the culture info lazy initializer
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture));

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        //GET
        protected override DriverResult Editor(DeploymentSubscriptionPart part, dynamic shapeHelper) {
            var contentTypes = new List<DeploymentContentType>();
            var queries = new List<DeploymentQuery>();
            List<IContent> deploymentConfigurations;

            switch (part.DeploymentType) {
                case DeploymentType.Export:
                    contentTypes = _orchardServices.ContentManager.GetContentTypeDefinitions()
                        .Select(c => new DeploymentContentType {Name = c.Name, DisplayName = c.DisplayName}).ToList();
                    queries = _orchardServices.ContentManager.Query("Query").List<QueryPart>().Select(q =>
                        new DeploymentQuery {
                            Name = q.Name,
                            Identity = _orchardServices.ContentManager.GetItemMetadata(q).Identity.ToString()
                        }).ToList();
                    deploymentConfigurations = _deploymentService.GetDeploymentTargetConfigurations();
                    break;
                case DeploymentType.Import:
                    var deploymentSource = _deploymentService.GetDeploymentSource(part.DeploymentConfiguration);
                    if (deploymentSource != null) {
                        contentTypes = deploymentSource.GetContentTypes().ToList();
                        queries = deploymentSource.GetQueries().ToList();
                    }
                    deploymentConfigurations = _deploymentService.GetDeploymentSourceConfigurations();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            queries.Insert(0, new DeploymentQuery {Identity = string.Empty, Name = "None"});

            var deploymentDescription = string.Empty;
            if (part.DeploymentConfiguration != null) {
                deploymentDescription = _orchardServices.ContentManager.GetItemMetadata(part.DeploymentConfiguration).DisplayText;
            }

            var viewModel = new SubscriptionPartViewModel {
                ContentItem = part.ContentItem,
                Metadata = part.IncludeMetadata,
                Data = part.IncludeData,
                Files = part.IncludeFiles,
                DeployAsDraft = part.DeployAsDrafts,
                ContentTypes = contentTypes,
                SelectedContentTypes = part.ContentTypes,
                CustomSteps = new List<CustomStepEntry>(),
                DeploymentType = part.DeploymentType.ToString(),
                DeploymentConfigurationId = part.DeploymentConfiguration != null ? part.DeploymentConfiguration.Id : 0,
                DeploymentDescription = deploymentDescription,
                FilterChoice = part.Filter.ToString(),
                DataImportChoice = part.VersionHistoryOption.ToString(),
                Queries = queries,
                SelectedQueryIdentity = part.QueryIdentity,
                DeploymentConfigurations = deploymentConfigurations
            };

            if (!part.DeployedChangesToUtc.HasValue) {
                return ContentShape("Parts_DeploymentSubscription_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Deployment.Subscription",
                        Model: viewModel,
                        Prefix: Prefix));
            }

            // date and time are formatted using the same patterns as DateTimePicker is, preventing other cultures issues
            var localDate = new Lazy<DateTime>(() => TimeZoneInfo.ConvertTimeFromUtc(
                part.DeployedChangesToUtc.Value,
                _orchardServices.WorkContext.CurrentTimeZone));

            viewModel.DeployedChangesToDisplay = string.Format("{0} {1}",
                localDate.Value.ToString("d", _cultureInfo.Value),
                localDate.Value.ToString("t", _cultureInfo.Value));

            return ContentShape("Parts_DeploymentSubscription_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Deployment.Subscription",
                    Model: viewModel,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            DeploymentSubscriptionPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new SubscriptionPartViewModel();

            if (!updater.TryUpdateModel(viewModel, Prefix, null, null)) return Editor(part, shapeHelper);
            
            part.IncludeMetadata = viewModel.Metadata;
            part.IncludeData = viewModel.Data;
            part.IncludeFiles = viewModel.Files;
            part.DeployAsDrafts = viewModel.DeployAsDraft;
            part.Filter = (FilterOptions) Enum.Parse(typeof (FilterOptions), viewModel.FilterChoice);
            part.VersionHistoryOption = (VersionHistoryOptions) Enum.Parse(typeof (VersionHistoryOptions), viewModel.DataImportChoice);
            part.ContentTypes = viewModel.SelectedContentTypes.ToList();
            part.QueryIdentity = viewModel.SelectedQueryIdentity;
            part.DeploymentConfiguration = _orchardServices.ContentManager.Get(viewModel.DeploymentConfigurationId);

            if (string.IsNullOrWhiteSpace(viewModel.DeployedChangesToDate)
                || string.IsNullOrWhiteSpace(viewModel.DeployedChangesToTime)) {
                return Editor(part, shapeHelper);
            }

            DateTime scheduled;
            var parseDateTime = String.Concat(viewModel.DeployedChangesToDate, " ", viewModel.DeployedChangesToTime);

            // use current culture
            if (!DateTime.TryParse(parseDateTime, _cultureInfo.Value, DateTimeStyles.None, out scheduled)) {
                return Editor(part, shapeHelper);
            }

            // the date time is entered locally for the configured timezone
            var timeZone = _orchardServices.WorkContext.CurrentTimeZone;

            part.DeployedChangesToUtc = TimeZoneInfo.ConvertTimeToUtc(scheduled, timeZone);

            return Editor(part, shapeHelper);
        }
    }
}
