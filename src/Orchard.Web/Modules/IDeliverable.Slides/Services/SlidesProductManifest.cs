using System.Collections.Generic;
using IDeliverable.Licensing;

namespace IDeliverable.Slides.Services
{
    public class SlidesProductManifest : ProductManifestProviderBase
    {
        public const int ProductId = 233554;
        public const string ExtensionName = "IDeliverable.Slides";
        public static readonly ProductManifest ProductManifest = new ProductManifest(ProductId, ExtensionName);

        public override IEnumerable<ProductManifest> GetProductManifests()
        {
            yield return ProductManifest;
        }
    }
}