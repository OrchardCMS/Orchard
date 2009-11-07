using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Mvc.ModelBinders {
    public class KeyedListModelBinder<T> : IModelBinder {
        private readonly ModelBinderDictionary _binders;
        private readonly ModelMetadataProvider _providers;
        private readonly Func<T, string> _keySelector;

        public KeyedListModelBinder(
            ModelBinderDictionary binders,
            ModelMetadataProvider providers,
            Func<T, string> keySelector) {
            _binders = binders;
            _providers = providers;
            _keySelector = keySelector;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            var binder = _binders.GetBinder(typeof(T));

            // given an existing collection
            var model = (IList<T>)bindingContext.Model;
            if (model != null) {
                foreach (var item in model) {
                    // only update non-null collections
                    if (Equals(item, default(T)))
                        continue;

                    // that have a key value
                    var key = _keySelector(item);
                    if (string.IsNullOrEmpty(key))
                        continue;

                    // use the configured binder to update the rest of the properties on the object
                    var currentItem = item;
                    var itemContext = new ModelBindingContext {
                        ModelMetadata = _providers.GetMetadataForType(() => currentItem, typeof(T)),
                        ModelName = bindingContext.ModelName + "[" + key + "]",
                        ModelState = bindingContext.ModelState,
                        PropertyFilter = bindingContext.PropertyFilter,
                        ValueProvider = bindingContext.ValueProvider,
                    };
                    binder.BindModel(controllerContext, itemContext);
                }
            }
            return model;
        }
    }
}
