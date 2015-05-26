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
    public class ContentFieldDisplay : ContentDisplayBase, IContentFieldDisplay {
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;

        public ContentFieldDisplay(
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator, 
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor, 
            IEnumerable<IContentFieldDriver> contentFieldDrivers) 
            : base(shapeFactory, shapeTableLocator, requestContext, virtualPathProvider, workContextAccessor) {

            _contentFieldDrivers = contentFieldDrivers;
        }

        public override string DefaultStereotype {
            get { return "ContentField"; }
        }

        public dynamic BuildDisplay(IContent content, ContentField field, string displayType, string groupId) {
            var context = BuildDisplayContext(content, displayType, groupId);
            var drivers = GetFieldDrivers(field.FieldDefinition.Name);

            // TODO: Make async all the way up. (see note on ContentPartDisplay.BuildDisplay)
            drivers.InvokeAsync(async driver => {
                var result = await driver.BuildDisplayShapeAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger).Wait();

            return context.Shape;
        }

        public dynamic BuildEditor(IContent content, ContentField field, string groupId) {
            var context = BuildEditorContext(content, groupId);
            var drivers = GetFieldDrivers(field.FieldDefinition.Name);

            // TODO: Make async all the way up. (see note on ContentPartDisplay.BuildDisplay)
            drivers.InvokeAsync(async driver => {
                var result = await driver.BuildEditorShapeAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger).Wait();
            

            return context.Shape;
        }

        public dynamic UpdateEditor(IContent content, ContentField field, IUpdateModel updater, string groupInfoId) {
            var context = UpdateEditorContext(content, updater, groupInfoId);
            var drivers = GetFieldDrivers(field.FieldDefinition.Name);

            // TODO: Make async all the way up. (see note on ContentPartDisplay.BuildDisplay)
            drivers.InvokeAsync(async driver => {
                var result = await driver.UpdateEditorShapeAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger).Wait();

            return context.Shape;
        }

        private IEnumerable<IContentFieldDriver> GetFieldDrivers(string fieldName) {
            return _contentFieldDrivers.Where(x => x.GetType().BaseType.GenericTypeArguments[0].Name == fieldName);
        }
    }
}
