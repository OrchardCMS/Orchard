using System.Linq;
using Orchard.Mvc.ModelMetadataProviders;

namespace Orchard.Localization {
    public class LocalizedModelMetadataProcessor : IModelMetadataProcessor {
        private readonly IWorkContextAccessor _workContextAccessor;

        public LocalizedModelMetadataProcessor(
            IWorkContextAccessor workContextAccessor
        ) {
            _workContextAccessor = workContextAccessor;
        }

        public void ProcessMetadata(ModelMetadataContext context) {

            var attribute = context.Attributes.OfType<LocalizedDisplayNameAttribute>().FirstOrDefault();
            if (attribute != null && attribute.DisplayName != null) {
                var scopeType = context.ContainerType ?? context.ModelType;
                var scope = scopeType != null ? scopeType.FullName : null;
                var localizer = LocalizationUtilities.Resolve(_workContextAccessor.GetContext(), scope);

                context.ModelMetadata.DisplayName = localizer(attribute.DisplayName).Text;
            }

        }
    }
}
