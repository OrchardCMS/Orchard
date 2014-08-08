using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Search.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Orchard.Search.Handlers {
    [OrchardFeature("Orchard.Search.Content")]
    public class AdminSearchSettingsPartHandler : ContentHandler {
        public AdminSearchSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<AdminSearchSettingsPart>("Site"));
            
            OnInitializing<AdminSearchSettingsPart>((context, part) => {
                part.FilterCulture = false;
                part.SearchedFields = new [] {"body, title"};
            });
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Admin Search")));
        }
    }
}