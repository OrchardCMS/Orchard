using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseSettingsPartHandlerBase<TPart> : ContentHandler where TPart : LicenseSettingsPartBase, new()
    {
        protected LicenseSettingsPartHandlerBase(IEnumerable<ILicensedProductManifest> products, IExtensionManager extensionManager)
        {
            Filters.Add(new ActivatingFilter<TPart>("Site"));
            OnGetContentItemMetadata<TPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Licenses"))));
            OnActivated<TPart>(SetupLazyFields);

            mProducts = products;
            mExtensionManager = extensionManager;
        }

        private readonly IEnumerable<ILicensedProductManifest> mProducts;
        private readonly IExtensionManager mExtensionManager;
        public Localizer T { get; set; } = NullLocalizer.Instance;

        private void SetupLazyFields(ActivatedContentContext context, TPart part)
        {
            part.mProductVersionField.Loader(value =>
            {
                var productManifest = mProducts.Single(x => x.ProductId == part.ProductId);
                var extension = mExtensionManager.GetExtension(productManifest.ExtensionId);

                return extension.Version;
            });
        }
    }
}