using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Search.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Search.Handlers {
    [UsedImplicitly]
    public class SearchSettingsPartHandler : ContentHandler {
        public SearchSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<SearchSettingsPart>("Site"));
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