using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Licensing
{
    public class LicensedProductManifest : Component, ILicensedProductManifest
    {
        public static readonly string ProductId = "233554";
        public static readonly string ProductName = "IDeliverable.Slides";
        public static readonly string ExtensionId = "IDeliverable.Slides";

        public LicensedProductManifest(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
        }

        private readonly IOrchardServices _orchardServices;
        string ILicensedProductManifest.ProductId => ProductId;
        string ILicensedProductManifest.ProductName => ProductName;
        string ILicensedProductManifest.ExtensionId => ExtensionId;
        bool ILicensedProductManifest.SkipValidationForLocalRequests => true;

        public string LicenseKey => _orchardServices.WorkContext.CurrentSite.As<SlidesLicenseSettingsPart>().LicenseKey;
    }
}