using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Framework.Elements {
    public class ElementDataDictionary : Dictionary<string, string> {
        public ElementDataDictionary() { }
        public ElementDataDictionary(IDictionary<string, string> dictionary) : base(dictionary) { }

        public T TryGetModel<T>(string key) where T : class {
            var binder = new DefaultModelBinder();
            var controllerContext = new ControllerContext();
            var context = new ModelBindingContext {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(T)),
                ModelName = key,
                ValueProvider = this.ToValueProvider(null)
            };
            return (T)binder.BindModel(controllerContext, context);

        }

        public T TryGetModel<T>() where T : class {
            return TryGetModel<T>(typeof(T).Name);
        }

        public T GetModel<T>() where T : class, new() {
            return GetModel<T>(typeof(T).Name);
        }

        public T GetModel<T>(string key) where T : class, new() {
            return TryGetModel<T>(key) ?? new T();
        }
    }
}