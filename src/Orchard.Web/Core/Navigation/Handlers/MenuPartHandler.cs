using System;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuPartHandler : ContentHandler {
        public MenuPartHandler(IRepository<MenuPartRecord> menuPartRepository) {
            Filters.Add(StorageFilter.For(menuPartRepository));

            OnInitializing<MenuPart>((ctx, x) => {
                                      x.MenuText = String.Empty;
                                  });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<MenuPart>();

            if (part != null) {
                string stereotype;
                if (context.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem") {
                    context.Metadata.DisplayText = part.MenuText;    
                }
            }
        }
    }
}