using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Services;
using Orchard.Data;
using Orchard.Core.Containers.Models;

namespace Orchard.Core.Containers.Handlers {
    public class ContainablePartHandler : ContentHandler {
        private readonly IContainerService _containerService;

        public ContainablePartHandler(IRepository<ContainablePartRecord> repository, IContainerService containerService) {
            _containerService = containerService;
            Filters.Add(StorageFilter.For(repository));

            OnCreated<ContainablePart>((context, part) => UpdateItemCount(part));
            OnPublished<ContainablePart>((context, part) => UpdateItemCount(part));
            OnUnpublished<ContainablePart>((context, part) => UpdateItemCount(part));
            OnVersioned<ContainablePart>((context, part, newVersionPart) => UpdateItemCount(newVersionPart));
            OnRemoved<ContainablePart>((context, part) => UpdateItemCount(part));
        }

        private void UpdateItemCount(ContainablePart part) {
            var commonPart = part.As<CommonPart>();
            if (commonPart == null || commonPart.Container == null)
                return;

            var containerPart = commonPart.Container.As<ContainerPart>();
            if (containerPart == null)
                return;

            _containerService.UpdateItemCount(containerPart);
        }
    }
}