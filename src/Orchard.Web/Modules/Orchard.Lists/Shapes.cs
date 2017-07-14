using System;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;

namespace Orchard.Lists {
    public class Shapes : IShapeTableProvider {
        private readonly Work<IContainerService> _containerService;

        public Shapes(Work<IContainerService> containerService) {
            _containerService = containerService;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Breadcrumbs_ContentItem").OnDisplaying(context => {
                // The breadcrumbs shape needs access to its items' containers latest version,
                // instead of the published version as implemented by CommonPart.Container's lazyload handler.
                // Instead of having this logic embedded in the view, we'll simply provide a pointer to it.
                context.Shape.ContainerAccessor = (Func<IContent, IContent>) (content => _containerService.Value.GetContainer(content, VersionOptions.Latest));
            });

            builder.Describe("ListNavigation").OnDisplaying(context => {
                var containable = (ContainablePart) context.Shape.ContainablePart;
                var container = _containerService.Value.GetContainer(containable, VersionOptions.Latest);
                if (container == null) return;

                var previous = _containerService.Value.Previous(container.Id, containable);
                var next = _containerService.Value.Next(container.Id, containable);

                context.Shape.Previous(previous);
                context.Shape.Next(next);
            });
        }
    }
}
