using System;
using System.Collections.Generic;
using Orchard;

namespace IDeliverable.Licensing.Orchard
{
    public interface IProductManifestManager
    {
        IEnumerable<ProductManifest> GetProductManifests();
        ProductManifest FindByProductId(int productId);
        ProductManifest FindByExtensionName(string extensionName);
        ProductManifest Find(Func<ProductManifest, bool> predicate);
    }
}