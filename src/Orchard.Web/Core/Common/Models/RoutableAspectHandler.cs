using System.Text;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Models {
    [UsedImplicitly]
    public class RoutableAspectHandler : ContentHandler
    {
        public RoutableAspectHandler() {
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