using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Orchard.Mvc.ModelBinders;

namespace Orchard.XmlRpc.Models {
    public class ModelBinderProvider : IModelBinderProvider, IModelBinder {
        private readonly IMapper<XElement, XRpcMethodCall> _mapper;

        public ModelBinderProvider(IMapper<XElement, XRpcMethodCall> mapper) {
            _mapper = mapper;
        }

        public IEnumerable<ModelBinderDescriptor> GetModelBinders() {
            return new[] {
                             new ModelBinderDescriptor {
                                                           ModelBinder =this,
                                                           Type=typeof(XRpcMethodCall)
                                                       }
                         };
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            //Ah! xmlrpc is a value provider!!!
            // TODO: refactor this? 
            using (var xmlReader = XmlReader.Create(controllerContext.HttpContext.Request.InputStream)) {
                var element = XElement.Load(xmlReader);
                return _mapper.Map(element);
            }
        }
    }
}
