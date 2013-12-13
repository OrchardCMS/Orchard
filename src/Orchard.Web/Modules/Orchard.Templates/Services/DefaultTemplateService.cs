using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Mvc;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services {
    public class DefaultTemplateService : ITemplateService {

        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayHelperFactory _displayHelperFactory;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ITemplateProcessor> _processors;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultTemplateService(
            IShapeFactory shapeFactory, 
            IDisplayHelperFactory displayHelperFactory, 
            IWorkContextAccessor workContextAccessor, 
            IContentManager contentManager, 
            IEnumerable<ITemplateProcessor> processors,
            IHttpContextAccessor httpContextAccessor) {
            _shapeFactory = shapeFactory;
            _displayHelperFactory = displayHelperFactory;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _processors = processors;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ExecuteShape(string shapeType) {
            return ExecuteShape(shapeType, null);
        }

        public string ExecuteShape(string shapeType, INamedEnumerable<object> parameters) {
            var shape = _shapeFactory.Create(shapeType, parameters);
            var result = "";

            ExecuteInHttpContext(httpContext => {
                var viewContext = new ViewContext { HttpContext = httpContext };
                viewContext.RouteData.DataTokens["IWorkContextAccessor"] = _workContextAccessor;
                var display = _displayHelperFactory.CreateHelper(viewContext, new ViewDataContainer());
                result = ((DisplayHelper)display).ShapeExecute(shape).ToString();
            });
            
            return result;
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

        /// <summary>
        /// Executes the action in an HttpContext, regardless of whether or not we're in a ThreadStaticScope or HttpContextScope.
        /// </summary>
        private void ExecuteInHttpContext(Action<HttpContextBase> action) {
            var httpContext = _httpContextAccessor.Current();

            if (httpContext != null) {
                action(httpContext);
                return;
            }

            // We're not using the ViewContext.HttpContext because that will be an EmptyHttpContext when we're on a background thread.
            // EmptyHttpContext does not implement Items, which is necessary for Orchard when storing and accessing certain services such as IWorkContextAccessor.
            httpContext = new StubHttpContext();
            using (var scope = _workContextAccessor.CreateWorkContextScope(httpContext)) {
                _httpContextAccessor.Set(httpContext);
                action(httpContext);
            }
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }

            public ViewDataContainer() {
                ViewData = new ViewDataDictionary();
            }
        }
    }
}