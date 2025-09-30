using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Containers {
    public class AdminMenu : INavigationProvider {
        private readonly IContainerService _containerService;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public AdminMenu(
            IContainerService containerService,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IWorkContextAccessor workContextAccessor) {
            _containerService = containerService;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("container");

            var containers = _containerService
                .GetContainersQuery(VersionOptions.Latest)
                .Where<ContainerPartRecord>(x => x.ShowOnAdminMenu)
                .List()
                .Where(content => _authorizationService.TryCheckAccess(
                    Contents.Permissions.EditContent,
                    _workContextAccessor.GetContext().CurrentUser,
                    content))
                .ToList();

            foreach (var container in containers) {
                var closureContainer = container;

                if (!string.IsNullOrWhiteSpace(container.AdminMenuImageSet)) {
                    builder.AddImageSet(container.AdminMenuImageSet.Trim());
                }

                builder.Add(T(container.AdminMenuText), container.AdminMenuPosition, item => {
                    var containedItems = _containerService.GetContentItems(closureContainer.Id, VersionOptions.Latest).ToList();
                    var actualContainer = closureContainer;
                    var position = 0;

                    // If the list has just a single item that happens to be a container itself,
                    // we will treat that one as the actual container to provide a nice & quick way to manage that list.
                    if (containedItems.Count == 1) {
                        var containedItem = containedItems.First().As<ContainerPart>();

                        if (containedItem != null) {
                            actualContainer = containedItem;
                            foreach (var itemContentType in containedItem.ItemContentTypes) {
                                var closureItemContentType = itemContentType;
                                item.Add(T("New {0}", itemContentType.DisplayName), string.Format("1.{0}", position++), subItem => subItem
                                    .Action("Create", "Admin", new {
                                        id = closureItemContentType.Name,
                                        containerid = containedItem.Id,
                                        area = "Contents"
                                    }));
                            }
                        }
                    }

                    item.Action(_contentManager.GetItemMetadata(actualContainer).AdminRouteValues)
                        .AddClass("section-container")
                        .AddClass(closureContainer.AdminMenuText.HtmlClassify())
                        .LinkToFirstChild(false);

                    foreach (var itemContentType in closureContainer.ItemContentTypes) {
                        var closureItemContentType = itemContentType;
                        item.Add(T("New {0}", itemContentType.DisplayName), string.Format("1.{0}", position++), subItem => subItem
                            .Action("Create", "Admin", new {
                                id = closureItemContentType.Name,
                                containerid = container.Id,
                                area = "Contents"
                            }));
                    }
                });
            }
        }
    }
}
