using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.ContentManagement.MetaData.Models {
    public class SettingsDictionary : Dictionary<string, string> {
        public SettingsDictionary() {}
        public SettingsDictionary(IDictionary<string, string> dictionary) : base(dictionary) {}

        public T GetModel<T>() {
            return GetModel<T>(null);
        }

        public T GetModel<T>(string key) {
            var binder = new DefaultModelBinder();
            var controllerContext = new ControllerContext();
            var context = new ModelBindingContext {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof (T)),
                ModelName = key,
                ValueProvider = new DictionaryValueProvider<string>(this, null)
            };
            return (T) binder.BindModel(controllerContext, context);
        }
    }
}