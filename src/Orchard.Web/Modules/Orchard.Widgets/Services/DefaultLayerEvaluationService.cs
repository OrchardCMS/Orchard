using System;
using System.Collections.Generic;
using Orchard.Conditions.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Widgets.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Caching;

namespace Orchard.Widgets.Services{
    public class DefaultLayerEvaluationService : ILayerEvaluationService {
        private readonly IConditionManager _conditionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        private readonly LazyField<int[]> _activeLayerIDs; 

        public DefaultLayerEvaluationService(
            IConditionManager conditionManager,
            IOrchardServices orchardServices,
            ICacheManager cacheManager,
            ISignals signals) {

            _conditionManager = conditionManager;
            _orchardServices = orchardServices;
            _cacheManager = cacheManager;
            _signals = signals;

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
            // Once the Condition Engine is done:
            // Get Layers and filter by zone and rule
            // NOTE: .ForType("Layer") is faster than .Query<LayerPart, LayerPartRecord>()
            var activeLayers = _cacheManager.Get(LayerPart.AllLayersCacheKey, true, ctx => {
                // invalidation signal 
                ctx.Monitor(_signals.When(LayerPart.AllLayersCacheEvictSignal));
                // cache
                return _orchardServices
                    .ContentManager
                    .Query<LayerPart>()
                    .ForType("Layer").List();
            });

            var activeLayerIds = new List<int>();
            foreach (var activeLayer in activeLayers) {
                // ignore the rule if it fails to execute
                try {
                    if (_conditionManager.Matches(activeLayer.LayerRule)) {
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
