using System;
using System.Collections.Generic;
using System.Web;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard;

namespace Orchard.Azure.MediaServices.Services.Assets {
    public interface IAssetManager : IDependency {
        Asset GetAssetById(int id);
        IEnumerable<Asset> LoadAssetsFor(CloudVideoPart part);
        IEnumerable<T> LoadAssetsFor<T>(CloudVideoPart part) where T:Asset;
        IEnumerable<Asset> LoadPendingAssets();
        Asset CreateAssetFor<T>(CloudVideoPart part, Action<T> initialize = null) where T : Asset, new();
        void DeleteAssetsFor(CloudVideoPart part);
        void DeleteAssets(IEnumerable<Asset> assets);
        void DeleteAsset(Asset asset);
        void PublishAssetsFor(CloudVideoPart part);
        void UnpublishAssetsFor(CloudVideoPart part);
        ThumbnailAsset GetThumbnailAssetFor(CloudVideoPart part);
        string SaveTemporaryFile(HttpPostedFileBase file);
    }
}
