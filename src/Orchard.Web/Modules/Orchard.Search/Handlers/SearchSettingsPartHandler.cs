using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Search.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Search.Handlers {
    [UsedImplicitly]
    public class SearchSettingsPartHandler : ContentHandler {
        public SearchSettingsPartHandler(IRepository<SearchSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<SearchSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Search")));
        }
    }
}