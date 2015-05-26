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

            // TODO: Make async all the way up. 
            // without making layouts module full async up and down (a large task)
            // present the synchronous interface at a low level to minimize impact.
            drivers.InvokeAsync(async driver => {
                var result = await driver.BuildDisplayAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger).Wait();
            

            return context.Shape;
        }

        public dynamic BuildEditor(ContentPart part, string groupId) {
            var context = BuildEditorContext(part, groupId);
            var drivers = GetPartDrivers(part.PartDefinition.Name);

            // TODO: Make async all the way up. (see note on ContentPartDisplay.BuildDisplay)
            drivers.InvokeAsync(async driver => {
                var result = await driver.BuildEditorAsync(context);
                if (result != null)
                    await result.ApplyAsync(context);
            }, Logger).Wait();

            return context.Shape;
        }

        public dynamic UpdateEditor(ContentPart part, IUpdateModel updater, string groupInfoId) {
            var context = UpdateEditorContext(part, updater, groupInfoId);
            var drivers = GetPartDrivers(part.PartDefinition.Name);

            // TODO: Make async all the way up. (see note on ContentPartDisplay.BuildDisplay)
            drivers.InvokeAsync(async driver => {
                    var result = await driver.UpdateEditorAsync(context);
                    if (result != null)
                        await result.ApplyAsync(context);
                }, Logger).Wait();

            return context.Shape;
        }

        private IEnumerable<IContentPartDriver> GetPartDrivers(string partName) {
            return _contentPartDrivers.Where(x => x.GetType().BaseType.GenericTypeArguments[0].Name == partName);
        }
    }
}
