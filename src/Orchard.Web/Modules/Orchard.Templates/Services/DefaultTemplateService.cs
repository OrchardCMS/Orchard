using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services {
    public class DefaultTemplateService : ITemplateService {

        public const string TemplatesSignal = "Orchard.Templates";

        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ITemplateProcessor> _processors;

        public DefaultTemplateService(
            IContentManager contentManager, 
            IEnumerable<ITemplateProcessor> processors) {
            _contentManager = contentManager;
            _processors = processors;
        }

        public string Execute<TModel>(string template, string name, string processorName, TModel model = default(TModel)) {
            return Execute(template, name, processorName, null, model);
        }

        public string Execute<TModel>(string template, string name, string processorName, DisplayContext context, TModel model = default(TModel)) {
            var processor = _processors.FirstOrDefault(x => String.Equals(x.Type, processorName, StringComparison.OrdinalIgnoreCase)) ?? _processors.First();
            return processor.Process(template, name, context, model);
        }

        public IEnumerable<ShapePart> GetTemplates(VersionOptions versionOptions = null) {
            return _contentManager.Query<ShapePart>(versionOptions ?? VersionOptions.Published).List();
        }

    }
}