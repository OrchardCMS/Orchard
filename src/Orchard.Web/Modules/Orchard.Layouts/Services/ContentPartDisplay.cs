using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Layouts.Services {
    public class ContentPartDisplay : ContentDisplayBase, IContentPartDisplay {
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;

        public ContentPartDisplay(
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator, 
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor, 
            IEnumerable<IContentPartDriver> contentPartDrivers) 
            : base(shapeFactory, shapeTableLocator, requestContext, virtualPathProvider, workContextAccessor) {

            _contentPartDrivers = contentPartDrivers;
        }

        public override string DefaultStereotype {
            get { return "ContentPart"; }
        }

        public dynamic BuildDisplay(ContentPart part, string displayType, string groupId) {
            var context = BuildDisplayContext(part, displayType, groupId);
            var drivers = GetPartDrivers(part.PartDefinition.Name);

            drivers.Invoke(driver => {
                var result = driver.BuildDisplay(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return context.Shape;
        }

        public dynamic BuildEditor(ContentPart part, string groupId) {
            var context = BuildEditorContext(part, groupId);
            var drivers = GetPartDrivers(part.PartDefinition.Name);

            drivers.Invoke(driver => {
                var result = driver.BuildEditor(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return context.Shape;
        }

        public dynamic UpdateEditor(ContentPart part, IUpdateModel updater, string groupInfoId) {
            var context = UpdateEditorContext(part, updater, groupInfoId);
            var drivers = GetPartDrivers(part.PartDefinition.Name);

            drivers.Invoke(driver => {
                var result = driver.UpdateEditor(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
            
            return context.Shape;
        }

        private IEnumerable<IContentPartDriver> GetPartDrivers(string partName) =>
            _contentPartDrivers.Where(x => GetPartOfDriver(x.GetType()?.BaseType)?.Name == partName);

        private Type GetPartOfDriver(Type type) {
            var baseType = type;

            while (baseType != null && typeof(IContentPartDriver).IsAssignableFrom(baseType)) {
                if (baseType.GenericTypeArguments.Any()) {
                    return baseType.GenericTypeArguments[0];
                }

                baseType = baseType.BaseType;
            }

            return null;
        }
    }
}
