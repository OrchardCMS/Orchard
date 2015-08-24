using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Orchard.Environment;

namespace Orchard.Mvc.ModelMetadataProviders {
    public class ModelMetadataProviderProxy : ModelMetadataProvider, IShim {
        private readonly IModelMetadataProvider _currentProvider;

        public ModelMetadataProviderProxy(
            ModelMetadataProvider currentProvider
        ) {
            _currentProvider = new ModelMetadataProviderWrapper(currentProvider ?? new EmptyModelMetadataProvider());
            OrchardHostContainerRegistry.RegisterShim(this);
        }

        public IOrchardHostContainer HostContainer { get; set; }

        private WorkContext GetWorkContext() {
            var ctx = HttpContext.Current;
            if (ctx == null)
                return null;

            var httpContextBase = new HttpContextWrapper(ctx);
            
            var runningShellTable = HostContainer.Resolve<IRunningShellTable>();
            if (runningShellTable == null)
                return null;

            var shellSettings = runningShellTable.Match(httpContextBase);
            if (shellSettings == null)
                return null;

            var orchardHost = HostContainer.Resolve<IOrchardHost>();
            if (orchardHost == null) {
                return null;
            }

            var shellContext = orchardHost.GetShellContext(shellSettings);
            if (shellContext == null) {
                return null;
            }

            var workContextAccessor = shellContext.LifetimeScope.Resolve<IWorkContextAccessor>();
            if (workContextAccessor == null) {
                return null;
            }

            return workContextAccessor.GetContext(httpContextBase);
        }

        private IModelMetadataProvider GetModelMetadataProvider() {
            var workContext = GetWorkContext();
            return workContext != null ? workContext.Resolve<IModelMetadataProvider>() : _currentProvider;
        }

        public override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType) {
            return GetModelMetadataProvider().GetMetadataForProperties(container, containerType);
        }

        public override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName) {
            return GetModelMetadataProvider().GetMetadataForProperty(modelAccessor, containerType, propertyName);
        }

        public override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType) {
            return GetModelMetadataProvider().GetMetadataForType(modelAccessor, modelType);
        }

        private class ModelMetadataProviderWrapper : IModelMetadataProvider {
            private readonly ModelMetadataProvider _metadataProvider;

            public ModelMetadataProviderWrapper(
                ModelMetadataProvider metadataProvider
            ) {
                _metadataProvider = metadataProvider;
            }

            public IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType) {
                return _metadataProvider.GetMetadataForProperties(container, containerType);
            }

            public ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName) {
                return _metadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName);
            }

            public ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType) {
                return _metadataProvider.GetMetadataForType(modelAccessor, modelType);
            }
        }
    }
}
