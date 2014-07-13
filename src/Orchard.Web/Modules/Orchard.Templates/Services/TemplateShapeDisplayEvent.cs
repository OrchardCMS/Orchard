using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Templates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Templates.Services {
    public class TemplateShapeDisplayEvent : IShapeDisplayEvents {
        private ICacheManager _cacheManager;
        private ISignals _signals;
        private IContentManager _contentManager;
        private ITemplateService _templateService;

        public TemplateShapeDisplayEvent(
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

        public void Displaying(ShapeDisplayingContext context) {
            var processors = BuildShapeProcessors();
            Func<dynamic, IHtmlString> processor;

            if (processors.TryGetValue(context.ShapeMetadata.Type, out processor)) {
                context.ChildContent = processor(context.Shape);
            }
        }

        public void Displayed(ShapeDisplayedContext context) {
        }

        public IDictionary<string, Func<dynamic, IHtmlString>> BuildShapeProcessors() {
            return _cacheManager.Get("Template.ShapeProcessors", ctx => {
                ctx.Monitor(_signals.When(DefaultTemplateService.TemplatesSignal));

                var allTemplates = _contentManager.Query<ShapePart>().List();

                return allTemplates.Select(x => new KeyValuePair<string, Func<dynamic, IHtmlString>>(
                    x.Name,
                    d => CoerceHtmlString(_templateService.Execute(x.Template, x.Name, x.ProcessorName, d))
                )).ToDictionary(x => x.Key, x => x.Value);
            });
        }

        private static IHtmlString CoerceHtmlString(object invoke) {
            return invoke as IHtmlString ?? (invoke != null ? new HtmlString(invoke.ToString()) : null);
        }
    }
}