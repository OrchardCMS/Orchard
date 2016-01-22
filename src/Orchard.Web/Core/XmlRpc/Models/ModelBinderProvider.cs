using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Orchard.Core.XmlRpc.Services;
using Orchard.Mvc.ModelBinders;
using IModelBinderProvider = Orchard.Mvc.ModelBinders.IModelBinderProvider;

namespace Orchard.Core.XmlRpc.Models {
    public class ModelBinderProvider : IModelBinderProvider, IModelBinder {
        private readonly IXmlRpcReader _mapper;

        public ModelBinderProvider(IXmlRpcReader mapper) {
            _mapper = mapper;
        }

        public IEnumerable<ModelBinderDescriptor> GetModelBinders() {
            return new[] {
                             new ModelBinderDescriptor {
                                                           ModelBinder = this,
                                                           Type = typeof(XRpcMethodCall)
                                                       }
                         };
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            // Ah! xmlrpc is a value provider!!!
            // TODO: refactor this? 
            using (var xmlReader = XmlReader.Create(controllerContext.HttpContext.Request.InputStream)) {
                var element = XElement.Load(xmlReader);
                return _mapper.MapToMethodCall(element);
            }
        }
    }
}