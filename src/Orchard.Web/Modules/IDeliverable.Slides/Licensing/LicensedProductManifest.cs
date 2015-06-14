using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Licensing
{
    public class LicensedProductManifest : ILicensedProductManifest
    {
        public static readonly string ProductId = "233554";
        public static readonly string ProductName = "IDeliverable.Slides";

        public LicensedProductManifest(IOrchardServices orchardServices)
        {
            _orchardServices = orchardServices;
        }

        private readonly IOrchardServices _orchardServices;
        string ILicensedProductManifest.ProductId => ProductId;
        string ILicensedProductManifest.ProductName => ProductName;
        bool ILicensedProductManifest.SkipValidationForLocalRequests => false;

        public string LicenseKey => _orchardServices.WorkContext.CurrentSite.As<SlidesLicenseSettingsPart>().LicenseKey;
    }
}