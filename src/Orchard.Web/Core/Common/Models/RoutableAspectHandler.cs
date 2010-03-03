using System.Text;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Records;
using Orchard.Data;

namespace Orchard.Core.Common.Models {
    [UsedImplicitly]
    public class RoutableAspectHandler : ContentHandler {
        public RoutableAspectHandler(IRepository<RoutableRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnGetEditorViewModel<RoutableAspect>((context, routable) => {
                var containerPathBuilder = new StringBuilder();
                var container = context.ContentItem.As<ICommonAspect>().Container;

                while (container != null) {
                    if (container.Is<RoutableAspect>())
                        containerPathBuilder.Insert(0, string.Format("{0}/", container.As<RoutableAspect>().Slug));
                    container = container.ContentItem.As<ICommonAspect>().Container;
                }

                routable.ContainerPath = containerPathBuilder.ToString();
            });
        }
    }
}