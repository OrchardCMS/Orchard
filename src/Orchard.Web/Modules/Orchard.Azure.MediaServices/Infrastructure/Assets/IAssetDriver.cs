using System.Collections.Generic;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard;
using Orchard.ContentManagement;

namespace Orchard.Azure.MediaServices.Infrastructure.Assets {
    public interface IAssetDriver : IDependency {
        IEnumerable<AssetDriverResult> BuildEditor(Asset asset, dynamic shapeFactory);
        IEnumerable<AssetDriverResult> UpdateEditor(Asset asset, IUpdateModel updater, dynamic shapeFactory);
    }
}