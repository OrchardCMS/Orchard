using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using TinyMce.Models;

namespace TinyMce.Handlers {
    [UsedImplicitly]
    [OrchardFeature("TinyMce.Settings")]
    public class TinyMceSettingsPartHandler : ContentHandler {        
        public TinyMceSettingsPartHandler() {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<TinyMceSettingsPart>("Site"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("TinyMCE")));
        }
    }
}