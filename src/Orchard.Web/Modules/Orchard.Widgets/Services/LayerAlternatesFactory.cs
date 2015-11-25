using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Widgets.Models;
using Orchard.ContentManagement;

namespace Orchard.Widgets.Services
{
    [OrchardFeature("Orchard.Widgets.LayerAlternates")]
    public class LayersAlternatesFactory : ShapeDisplayEvents
    {
        private readonly ILayerEvaluationService _layerEvaluationService;
        private readonly Lazy<List<string>> _layersAlternates;
        private readonly IOrchardServices _orchardServices;


        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public LayersAlternatesFactory(ILayerEvaluationService layerEvaluationService, IOrchardServices orchardServices)
        {
            _layerEvaluationService = layerEvaluationService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _layersAlternates = new Lazy<List<string>>(() => {
                int[] activeLayers = _layerEvaluationService.GetActiveLayerIds();

                if (activeLayers == null || activeLayers.Length == 0)
                {
                    return null;
                }

                // Get Layer names and remove any ' ', '.'
                var layers = _orchardServices.ContentManager.GetMany<LayerPart>(activeLayers, VersionOptions.Published, QueryHints.Empty)
                        .Where(l => !l.Name.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                        .Select(l => l.Name.Replace(" ", "__").Replace(".", "_"))
                        .ToArray();

                var lyrs = Enumerable.Range(1, layers.Count()).Select(range => String.Join("__", layers.Take(range))).Union(layers).ToList();
                return lyrs;
            });
        }

        public override void Displaying(ShapeDisplayingContext context)
        {

            context.ShapeMetadata.OnDisplaying(displayedContext => {

                if (_layersAlternates.Value == null || !_layersAlternates.Value.Any())
                {
                    return;
                }

                // prevent applying alternate again, c.f. https://github.com/OrchardCMS/Orchard/issues/2125
                if (displayedContext.ShapeMetadata.Alternates.Any(x => x.Contains("__layer__")))
                {
                    return;
                }

                // appends Url alternates to current ones
                displayedContext.ShapeMetadata.Alternates = displayedContext.ShapeMetadata.Alternates.SelectMany(
                    alternate => new[] { alternate }.Union(_layersAlternates.Value.Select(a => alternate + "__layer__" + a))
                    ).ToList();

                // appends [ShapeType]__url__[Url] alternates
                displayedContext.ShapeMetadata.Alternates = _layersAlternates.Value.Select(url => displayedContext.ShapeMetadata.Type + "__layer__" + url)
                    .Union(displayedContext.ShapeMetadata.Alternates)
                    .ToList();
            });

        }
    }
}