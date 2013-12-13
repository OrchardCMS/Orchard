using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services {
    public class DefaultTemplateService : ITemplateService {

        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayHelperFactory _displayHelperFactory;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ITemplateProcessor> _processors;
        private readonly HttpContextBase _httpContextBase;

        public DefaultTemplateService(
            IShapeFactory shapeFactory, 
            IDisplayHelperFactory displayHelperFactory, 
            IWorkContextAccessor workContextAccessor, 
            IContentManager contentManager, 
            IEnumerable<ITemplateProcessor> processors,
            HttpContextBase httpContextBase) {
            _shapeFactory = shapeFactory;
            _displayHelperFactory = displayHelperFactory;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _processors = processors;
            _httpContextBase = httpContextBase;
        }

        public string ExecuteShape(string shapeType) {
            return ExecuteShape(shapeType, null);
        }

        public string ExecuteShape(string shapeType, INamedEnumerable<object> parameters) {
            var shape = _shapeFactory.Create(shapeType, parameters);

            var viewContext = new ViewContext { HttpContext = _httpContextBase };
            viewContext.RouteData.DataTokens["IWorkContextAccessor"] = _workContextAccessor;
            var display = _displayHelperFactory.CreateHelper(viewContext, new ViewDataContainer());
            
            return ((DisplayHelper)display).ShapeExecute(shape).ToString();
        }

        public string Execute<TModel>(string template, string name, string language, TModel model = default(TModel)) {
            return Execute(template, name, language, null, model);
        }

        public string Execute<TModel>(string template, string name, string language, DisplayContext context, TModel model = default(TModel)) {
            var processor = _processors.FirstOrDefault(x => String.Equals(x.Type, language, StringComparison.OrdinalIgnoreCase));
            return processor != null ? processor.Process(template, name, context, model) : string.Empty;
        }

        public IEnumerable<ShapePart> GetTemplates(VersionOptions versionOptions = null) {
            return _contentManager.Query<ShapePart>(versionOptions ?? VersionOptions.Published).List();
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }

            public ViewDataContainer() {
                ViewData = new ViewDataDictionary();
            }
        }
    }
}