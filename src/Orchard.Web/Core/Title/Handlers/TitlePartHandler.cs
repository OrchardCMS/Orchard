using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace Orchard.Core.Title.Handlers {
    public class TitlePartHandler : ContentHandler {

        public TitlePartHandler(IRepository<TitlePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
