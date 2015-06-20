using System.Collections.Generic;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Models;
using Orchard.Environment.Extensions;

namespace IDeliverable.Slides.Handlers
{
    public class SlidesLicenseSettingsPartHandler : LicenseSettingsPartHandlerBase<SlidesLicenseSettingsPart>
    {
        public SlidesLicenseSettingsPartHandler(IEnumerable<ILicensedProductManifest> products, IExtensionManager extensionManager) 
            : base(products, extensionManager)
        {
        }
    }
}