using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDeliverable.Licensing.Orchard;

namespace IDeliverable.Slides.Licensing
{
    public class LicensedProductManifest : ILicensedProductManifest
    {
        public static readonly string ProductId = "233554";
        public static readonly string ProductName = "IDeliverable.Slides";

        string ILicensedProductManifest.ProductId => ProductId;
        string ILicensedProductManifest.ProductName => ProductName;
        bool ILicensedProductManifest.SkipValidationForLocalRequests => true;

        public string LicenseKey => ""; // TODO: Sipke get this from some kind of site settings.
    }
}