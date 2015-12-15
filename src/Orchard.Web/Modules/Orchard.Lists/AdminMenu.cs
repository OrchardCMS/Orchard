using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.Lists {
    public class AdminMenu : INavigationProvider {
        private readonly IContainerService _containerService;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public AdminMenu(
            IContainerService containerService, 
            IContentManager contentManager,
            IAuthorizationService authorizationService, 
            IWorkContextAccessor workContextAccessor
            ) {
            _containerService = containerService;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("list");

            CreateListManagementMenuItem(builder);
            CreateListMenuItems(builder);
        }

        private void CreateListManagementMenuItem(NavigationBuilder builder) {
            builder.Add(T("Lists"), "11", item => item
                .Action("Index", "Admin", new {area = "Orchard.Lists"}).Permission(Permissions.ManageLists)
            );
        }

        private void CreateListMenuItems(NavigationBuilder builder) {
            var containers = _containerService
                .GetContainersQuery(VersionOptions.Latest)
                .Where<ContainerPartRecord>(x => x.ShowOnAdminMenu)
                .List()
                .Where(x => _authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.EditContent, _workContextAccessor.GetContext().CurrentUser, x))
                .ToList();

            foreach (var container in containers) {
                var closureContainer = container;

                if (!String.IsNullOrWhiteSpace(container.AdminMenuImageSet)) {
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
                                item.Add(T("New {0}", itemContentType.DisplayName), String.Format("1.{0}", position++), subItem => subItem
                                    .Action("Create", "Admin", new { id = closureItemContentType.Name, containerid = containedItem.Id, area = "Contents" }));
                            }
                        }
                    }

                    var containerMetadata = _contentManager.GetItemMetadata(actualContainer);
                    item.Action(containerMetadata.AdminRouteValues);

                    item.Action(containerMetadata.AdminRouteValues);
                    item.AddClass("nav-list");
                    item.AddClass(closureContainer.AdminMenuText.HtmlClassify());
                    item.LinkToFirstChild(false);
                    
                    foreach (var itemContentType in closureContainer.ItemContentTypes) {
                        var closureItemContentType = itemContentType;
                        item.Add(T("New {0}", itemContentType.DisplayName), String.Format("1.{0}", position++), subItem => subItem
                            .Action("Create", "Admin", new { id = closureItemContentType.Name, containerid = container.Id, area = "Contents" }));
                    }
                });
            }
        }
    }
}