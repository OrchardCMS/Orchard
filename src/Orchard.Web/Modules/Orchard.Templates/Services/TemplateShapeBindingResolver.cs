using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.Implementation;
using Orchard.Templates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using System.Web.Routing;
using Orchard.UI.Admin;

namespace Orchard.Templates.Services {
    public class TemplateShapeBindingResolver : IShapeBindingResolver {
        private ICacheManager _cacheManager;
        private ISignals _signals;
        private IContentManager _contentManager;
        private IContentDefinitionManager _contentDefinitionManager;
        private ITemplateService _templateService;
        private readonly RequestContext _requestContext;

        public TemplateShapeBindingResolver(
            ICacheManager cacheManager,
            ISignals signals,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITemplateService templateService,
            RequestContext requestContext) {
            _cacheManager = cacheManager;
            _signals = signals;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _templateService = templateService;
            _requestContext = requestContext;
        }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding) {
            var processors = BuildShapeProcessors();

            var acceptableRenderingModes = new List<RenderingMode>() { RenderingMode.FrontEndAndAdmin };
            if (AdminFilter.IsApplied(_requestContext)) {
                acceptableRenderingModes.Add(RenderingMode.Admin);
            }
            else {
                acceptableRenderingModes.Add(RenderingMode.FrontEnd);
            }

            var templateResults = processors[shapeType].Where(template => acceptableRenderingModes.Contains(template.RenderingMode));
            TemplateResult templateResult = null;
            var templateResultsCount = templateResults.Count();
            if (templateResultsCount == 1) {
                templateResult = templateResults.FirstOrDefault();
            }
            else if (templateResultsCount > 1) {
                // Templates with the same name but specified rendering mode are prioritized.
                templateResult = templateResults.FirstOrDefault(template => template.RenderingMode != RenderingMode.FrontEndAndAdmin);
            }

            if (templateResult != null) {
                shapeBinding = new ShapeBinding {
                    BindingName = "Templates",
                    Binding = ctx => CoerceHtmlString(_templateService.Execute(
                        templateResult.Template,
                        templateResult.Name,
                        templateResult.Processor, ctx.Value)),
                    ShapeDescriptor = new ShapeDescriptor { ShapeType = shapeType }
                };

                return true;
            }

            shapeBinding = null;
            return false;
        }

        private ILookup<string, TemplateResult> BuildShapeProcessors() {
            return _cacheManager.Get("Template.ShapeProcessors", true, ctx => {
                ctx.Monitor(_signals.When(DefaultTemplateService.TemplatesSignal));

                // select all name of types which contains ShapePart
                var typesWithShapePart = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(ct => ct.Parts.Any(cp => cp.PartDefinition.Name == "ShapePart"))
                    .Select(ct => ct.Name)
                    .ToArray();

                var allTemplates = _contentManager.Query<ShapePart>(typesWithShapePart).List();

                return allTemplates.Select(shapePart => new TemplateResult {
                    Name = shapePart.Name,
                    Template = shapePart.Template,
                    Processor = shapePart.ProcessorName,
                    RenderingMode = shapePart.RenderingMode
                }).ToLookup(template => template.Name);
            });
        }

        private static IHtmlString CoerceHtmlString(object invoke) {
            return invoke as IHtmlString ?? (invoke != null ? new HtmlString(invoke.ToString()) : null);
        }

        private class TemplateResult {
            public string Name { get; set; }
            public string Processor { get; set; }
            public string Template { get; set; }
            public RenderingMode RenderingMode { get; set; }
        }
    }
}