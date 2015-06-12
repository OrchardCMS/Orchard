using System;
using System.Collections.Generic;
using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public interface IProductManifestManager
    {
        IEnumerable<ProductManifest> GetProductManifests();
        ProductManifest FindByProductId(int productId);
        ProductManifest FindByExtensionName(string extensionName);
        ProductManifest Find(Func<ProductManifest, bool> predicate);
    }
}