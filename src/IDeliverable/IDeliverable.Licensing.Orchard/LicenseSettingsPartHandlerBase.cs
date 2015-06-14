using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseSettingsPartHandlerBase<TPart> : ContentHandler where TPart : LicenseSettingsPartBase, new()
    {
        protected LicenseSettingsPartHandlerBase()
        {
            Filters.Add(new ActivatingFilter<TPart>("Site"));
            OnGetContentItemMetadata<TPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Licenses"))));
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
    }
}