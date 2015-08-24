using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Orchard.Mvc.ModelMetadataProviders {
    public class DefaultModelMetadataProvider : DataAnnotationsModelMetadataProvider, IModelMetadataProvider {
        private readonly IEnumerable<IModelMetadataProcessor> _modelMetadataProviders;

        public DefaultModelMetadataProvider(
            IEnumerable<IModelMetadataProcessor> modelMetadataProviders 
        ) {
            _modelMetadataProviders = modelMetadataProviders;
        }

        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName) {
            var attributeList = attributes.ToArray();

            var data = base.CreateMetadata(attributeList, containerType, modelAccessor, modelType, propertyName);

            var context = new ModelMetadataContext {
                ModelMetadata = data,
                Attributes = attributeList,
                ContainerType = containerType,
                ModelAccessor = modelAccessor,
                ModelType = modelType,
                PropertyName = propertyName,
            };
            foreach (var modelMetadataProvider in _modelMetadataProviders) {
                modelMetadataProvider.ProcessMetadata(context);
            }

            return data;
        }
    }
}
