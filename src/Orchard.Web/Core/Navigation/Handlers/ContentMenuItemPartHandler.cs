using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class ContentMenuItemPartHandler : ContentHandler {

        public ContentMenuItemPartHandler(IContentManager contentManager, IRepository<ContentMenuItemPartRecord> repository) {
            Filters.Add(new ActivatingFilter<ContentMenuItemPart>("ContentMenuItem"));
            Filters.Add(StorageFilter.For(repository));

            OnLoading<ContentMenuItemPart>((context, part) => part._content.Loader(p => contentManager.Get(part.Record.ContentMenuItemRecord.Id)));
        }
    }
}