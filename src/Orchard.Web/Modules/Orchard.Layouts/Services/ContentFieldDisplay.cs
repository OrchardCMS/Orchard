using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.VirtualPath;
using Orchard.Mvc.Routes;

namespace Orchard.Layouts.Services {
    public class ContentFieldDisplay : ContentDisplayBase, IContentFieldDisplay {
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly ShellSettings _shellSettings;
        public ContentFieldDisplay(
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor,
            ShellSettings shellSettings,
            IEnumerable<IContentFieldDriver> contentFieldDrivers)
            : base(shapeFactory, shapeTableLocator, requestContext, virtualPathProvider, workContextAccessor) {
            _shellSettings = shellSettings;
            _contentFieldDrivers = contentFieldDrivers;
        }

        public override UrlPrefix TenantUrlPrefix {
            get {
                if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix)) {
                    return new UrlPrefix(_shellSettings.RequestUrlPrefix);
                }
                else {
                    return null;
                }
            }
        }


        public override string DefaultStereotype {
            get { return "ContentField"; }
        }

        public dynamic BuildDisplay(IContent content, ContentField field, string displayType, string groupId) {
            var context = BuildDisplayContext(content, displayType, groupId);
            var drivers = GetFieldDrivers(field.FieldDefinition.Name);

            drivers.Invoke(driver => {
                var result = Filter(driver.BuildDisplayShape(context), field);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return context.Shape;
        }

        public dynamic BuildEditor(IContent content, ContentField field, string groupId) {
            var context = BuildEditorContext(content, groupId);
            var drivers = GetFieldDrivers(field.FieldDefinition.Name);

            drivers.Invoke(driver => {
                var result = driver.BuildEditorShape(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, ContentField field, IUpdateModel updater, string groupInfoId) {
            var context = UpdateEditorContext(content, updater, groupInfoId);
            var drivers = GetFieldDrivers(field.FieldDefinition.Name);

            drivers.Invoke(driver => {
                var result = driver.UpdateEditorShape(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return context.Shape;
        }

        private DriverResult Filter(DriverResult driverResult, ContentField field) {
            DriverResult result = null;
            var combinedResult = driverResult as CombinedResult;
            var contentShapeResult = driverResult as ContentShapeResult;

            if (combinedResult != null) {
                result = combinedResult.GetResults().SingleOrDefault(x => x.ContentField != null && x.ContentField.Name == field.Name);
            }
            else if (contentShapeResult != null) {
                result = contentShapeResult.ContentField != null && contentShapeResult.ContentField.Name == field.Name ? contentShapeResult : driverResult;
            }

            return result;
        }

        private IEnumerable<IContentFieldDriver> GetFieldDrivers(string fieldName) {
            return _contentFieldDrivers.Where(x => x.GetType().BaseType.GenericTypeArguments[0].Name == fieldName);
        }
    }
}
