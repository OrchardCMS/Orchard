using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.Core.Navigation
{
    public class DynamicPermissions : IPermissionProvider
    {
        private static readonly Permission ManageMenu = new Permission { Description = "Manage '{0}' menu", Name = "Manage_{0}", ImpliedBy = new[] { Permissions.ManageMenus } };

        private readonly IMenuService _menuService;
        private readonly IContentManager _contentManager;

        public virtual Feature Feature { get; set; }

        public DynamicPermissions(
            IMenuService menuService,
            IContentManager contentManager)
        {
            _menuService = menuService;
            _contentManager = contentManager;
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return _menuService.GetMenus().Select(menu => CreateMenuPermission(menu, _contentManager));
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return Enumerable.Empty<PermissionStereotype>();
        }

        public static Permission CreateMenuPermission(ContentItem menu, IContentManager contentManager)
        {
            var metadata = contentManager.GetItemMetadata(menu);

            return new Permission
            {
                Name = string.Format(ManageMenu.Name, metadata.Identity),
                Description = string.Format(ManageMenu.Description, metadata.DisplayText),
                Category = "Navigation Feature",
                ImpliedBy = new[] { Permissions.ManageMenus }
            };
        }
    }
}
