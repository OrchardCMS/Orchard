using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Mvc.ModelBinders;

namespace Orchard.CmsPages.Models {
    public class ContentItemListModelBinder : IModelBinderProvider {
        private readonly ModelBinderDictionary _binders;
        private readonly ModelMetadataProvider _provider;

        public ContentItemListModelBinder(ModelBinderDictionary binders, ModelMetadataProvider provider) {
            _binders = binders;
            _provider = provider;
        }


        public IEnumerable<ModelBinderDescriptor> GetModelBinders() {
            yield return new ModelBinderDescriptor {
                Type = typeof(IList<ContentItem>),
                ModelBinder = new KeyedListModelBinder<ContentItem>(_binders, _provider, item => item.ZoneName)
            };
        }
    }
}
