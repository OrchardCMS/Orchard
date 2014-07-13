using System.Collections.Generic;
using Orchard.Azure.MediaServices.Infrastructure.Assets;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.ContentManagement;

namespace Orchard.Azure.MediaServices.Drivers {
    public class VideoAssetDriver : AssetDriver<VideoAsset> {
        protected override IEnumerable<AssetDriverResult> Editor(VideoAsset asset, dynamic shapeFactory) {
            return Editor(asset, null, shapeFactory);
        }

        protected override IEnumerable<AssetDriverResult> Editor(VideoAsset asset, IUpdateModel updater, dynamic shapeFactory) {
            yield return new AssetDriverResult {
                TabTitle = T("Files"),
                EditorShape = shapeFactory.EditorTemplate(Model: asset, TemplateName: "Assets/Video.Files", Prefix: Prefix)
            };
            yield return new AssetDriverResult {
                TabTitle = T("Preview"),
                EditorShape = shapeFactory.EditorTemplate(Model: asset, TemplateName: "Assets/Video.Preview", Prefix: Prefix)
            };
        }
    }
}