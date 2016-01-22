using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Services {
    /// <summary>
    /// Implements <see cref="IMenuManager"/> by searching for the <code>MenuItem</code> stereotype in the content type's settings.
    /// </summary>
    public class DefaultMenuManager : IMenuManager {
        private readonly IContentManager _contentManager;

        public DefaultMenuManager(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<MenuItemDescriptor> GetMenuItemTypes() {
            return _contentManager.GetContentTypeDefinitions()
                .Where(contentTypeDefinition => contentTypeDefinition.Settings.ContainsKey("Stereotype") && contentTypeDefinition.Settings["Stereotype"] == "MenuItem")
                .Select(contentTypeDefinition =>
                    new MenuItemDescriptor {
                        Type = contentTypeDefinition.Name,
                        DisplayName = contentTypeDefinition.DisplayName,
                        Description = contentTypeDefinition.Settings.ContainsKey("Description") ? contentTypeDefinition.Settings["Description"] : null
                    });
        }
    }

}