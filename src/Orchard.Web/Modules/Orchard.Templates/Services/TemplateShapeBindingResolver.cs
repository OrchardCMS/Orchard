using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Templates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Templates.Services {
    public class TemplateShapeBindingResolver : IShapeBindingResolver {
        private ICacheManager _cacheManager;
        private ISignals _signals;
        private IContentManager _contentManager;
        private ITemplateService _templateService;

        public TemplateShapeBindingResolver(
            ICacheManager cacheManager,
            ISignals signals,
            IContentManager contentManager,
            ITemplateService templateService
            ) {
            _cacheManager = cacheManager;
            _signals = signals;
            _contentManager = contentManager;
            _templateService = templateService;
        }

        public bool TryGetDescriptorBinding(string shapeType, out ShapeBinding shapeBinding) {
            var processors = BuildShapeProcessors();

            TemplateResult templateResult = null;
            if (processors.TryGetValue(shapeType, out templateResult)) {
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

        private IDictionary<string, TemplateResult> BuildShapeProcessors() {
            return _cacheManager.Get("Template.ShapeProcessors", ctx => {
                ctx.Monitor(_signals.When(DefaultTemplateService.TemplatesSignal));

                var allTemplates = _contentManager.Query<ShapePart>().List();

                return allTemplates.Select(x => {
                    var name = x.Name;
                    var template = x.Template;
                    var processorName = x.ProcessorName;

                    return new TemplateResult {
                        Name = x.Name,
                        Template = x.Template,
                        Processor = x.ProcessorName
                    };

                }).ToDictionary(x => x.Name, x => x);
            });
        }

        private static IHtmlString CoerceHtmlString(object invoke) {
            return invoke as IHtmlString ?? (invoke != null ? new HtmlString(invoke.ToString()) : null);
        }

        private class TemplateResult {
            public string Name { get; set; }
            public string Processor { get; set; }
            public string Template { get; set; }
        }
    }
}