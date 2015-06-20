using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Licensing;

namespace IDeliverable.Slides.Models
{
    public class SlidesLicenseSettingsPart : LicenseSettingsPartBase
    {
        public override string ProductId => LicensedProductManifest.ProductId;
    }
}