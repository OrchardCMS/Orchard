using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using TinyMce.Models;

namespace TinyMce.Handlers
{
    public class TinyMceSettingsPartHandler : ContentHandler
    {
        public TinyMceSettingsPartHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<TinyMceSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<TinyMceSettingsPart>("TinyMceSettings", "Parts.TinyMce.TinyMceSettings", "TinyMCE"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("TinyMCE")));
        }
    }
}
