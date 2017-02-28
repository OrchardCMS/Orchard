using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Conditions.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;
using Orchard.Glimpse.Services;
using Orchard.Glimpse.Tabs.Layers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Html;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Glimpse.AlternateImplementation {
    [OrchardFeature(FeatureNames.Layers)]
    [OrchardSuppressDependency("Orchard.Widgets.Services.DefaultLayerEvaluationService")]
    public class GlimpseLayerEvaluationService : ILayerEvaluationService {
        private readonly IGlimpseService _glimpseService;
        private readonly IOrchardServices _orchardServices;
        private readonly UrlHelper _urlHelper;
        private readonly IConditionManager _conditionManager;

        private readonly LazyField<int[]> _activeLayerIDs;

        public GlimpseLayerEvaluationService(IGlimpseService glimpseService, IOrchardServices orchardServices, UrlHelper urlHelper, IConditionManager conditionManager) {
            _glimpseService = glimpseService;
            _orchardServices = orchardServices;
            _urlHelper = urlHelper;
            _conditionManager = conditionManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;

            _activeLayerIDs = new LazyField<int[]>();
            _activeLayerIDs.Loader(PopulateActiveLayers);
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        /// <summary>
        /// Retrieves every Layer from the Content Manager and evaluates each one.
        /// </summary>
        /// <returns>
        /// A collection of integers that represents the Ids of each active Layer
        /// </returns>
        public int[] GetActiveLayerIds() {
            return _activeLayerIDs.Value;
        }

        private int[] PopulateActiveLayers() {
            // Once the Rule Engine is done:
            // Get Layers and filter by zone and rule
            // NOTE: .ForType("Layer") is faster than .Query<LayerPart, LayerPartRecord>()
            var activeLayers = _orchardServices.ContentManager.Query<LayerPart>().WithQueryHints(new QueryHints().ExpandParts<LayerPart>()).ForType("Layer").List();

            var activeLayerIds = new List<int>();
            foreach (var activeLayer in activeLayers) {
                // ignore the rule if it fails to execute
                try {
                    var currentLayer = activeLayer;
                    var layerRuleMatches = _glimpseService.PublishTimedAction(() => _conditionManager.Matches(currentLayer.Record.LayerRule), (r, t) => new LayerMessage {
                        Active = r,
                        Name = currentLayer.Record.Name,
                        Rule = currentLayer.Record.LayerRule,
                        EditUrl = GlimpseHelpers.AppendReturnUrl(_urlHelper.ItemAdminUrl(activeLayer), _urlHelper),
                        Duration = t.Duration
                    }, TimelineCategories.Layers, "Layer Evaluation", currentLayer.Record.Name).ActionResult;

                    if (layerRuleMatches) {
                        activeLayerIds.Add(activeLayer.ContentItem.Id);
                    }
                }
                catch (Exception e) {
                    Logger.Warning(e, T("An error occurred during layer evaluation on: {0}", activeLayer.Name).Text);
                }
            }

            return activeLayerIds.ToArray();
        }
    }
}