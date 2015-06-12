using System.Collections.Generic;
using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public interface IProductManifestProvider
    {
        IEnumerable<ProductManifest> GetProductManifests();
    }
}