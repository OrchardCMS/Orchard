using System;
using System.Linq;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Models.Assets;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Azure.MediaServices.Shapes {
    public class CloudVideoPlayerShape : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("CloudVideoPlayer").OnDisplaying(context => {
                var shape = context.Shape;
                CloudVideoPart cloudVideoPart = shape.CloudVideoPart; // The cloud video item to render a player for.
                int? assetId = shape.AssetId; // Set to limit the player to only one particular asset.
                bool ignoreIncludeInPlayer = shape.IgnoreIncludeInPlayer; // True to ignore the IncludeInPlayer property of assets.
                bool allowPrivateUrls = shape.AllowPrivateUrls; // True to allow private locator URLs to be used, otherwise false.

                Func<string, string, string> selectUrl = (privateUrl, publicUrl) => publicUrl ?? (allowPrivateUrls ? privateUrl : null);

                var videoAssetQuery =
                    from asset in cloudVideoPart.Assets
                    where
                        (asset.IncludeInPlayer || ignoreIncludeInPlayer) &&
                        asset is VideoAsset && !(asset is DynamicVideoAsset) &&
                        (!assetId.HasValue || asset.Record.Id == assetId.Value)
                    let videoAsset = asset as VideoAsset
                    select new {
                        Type = videoAsset.GetType().Name,
                        Id = videoAsset.Record.Id,
                        Name = videoAsset.Name,
                        MimeType = videoAsset.MimeType,
                        MediaQuery = videoAsset.MediaQuery,
                        EncodingPreset = videoAsset.EncodingPreset,
                        EncoderMetadata = videoAsset.EncoderMetadata,
                        MainFileUrl = selectUrl(videoAsset.PrivateMainFileUrl, videoAsset.PublicMainFileUrl)
                    };

                var dynamicVideoAssetQuery =
                    from asset in cloudVideoPart.Assets
                    where
                        (asset.IncludeInPlayer || ignoreIncludeInPlayer) &&
                        asset is DynamicVideoAsset &&
                        (!assetId.HasValue || asset.Record.Id == assetId.Value)
                    let videoAsset = asset as DynamicVideoAsset
                    select new {
                        Type = videoAsset.GetType().Name,
                        Id = videoAsset.Record.Id,
                        Name = videoAsset.Name,
                        MimeType = videoAsset.MimeType,
                        MediaQuery = videoAsset.MediaQuery,
                        EncodingPreset = videoAsset.EncodingPreset,
                        EncoderMetadata = videoAsset.EncoderMetadata,
                        MainFileUrl = selectUrl(videoAsset.PrivateMainFileUrl, videoAsset.PublicMainFileUrl),
                        SmoothStreamingUrl = selectUrl(videoAsset.PrivateSmoothStreamingUrl, videoAsset.PublicSmoothStreamingUrl),
                        HlsUrl = selectUrl(videoAsset.PrivateHlsUrl, videoAsset.PublicHlsUrl),
                        MpegDashUrl = selectUrl(videoAsset.PrivateMpegDashUrl, videoAsset.PublicMpegDashUrl)
                    };

                var thumbnailAssetQuery =
                from asset in cloudVideoPart.Assets
                where
                    (asset.IncludeInPlayer || ignoreIncludeInPlayer) &&
                    asset is ThumbnailAsset &&
                    (!assetId.HasValue || asset.Record.Id == assetId.Value)
                let thumbnailAsset = asset as ThumbnailAsset
                select new {
                    Type = thumbnailAsset.GetType().Name,
                    Id = thumbnailAsset.Record.Id,
                    Name = thumbnailAsset.Name,
                    MimeType = thumbnailAsset.MimeType,
                    MediaQuery = thumbnailAsset.MediaQuery,
                    MainFileUrl = selectUrl(thumbnailAsset.PrivateMainFileUrl, thumbnailAsset.PublicMainFileUrl)
                };

                var subtitleAssetQuery =
                    from asset in cloudVideoPart.Assets
                    where
                        (asset.IncludeInPlayer || ignoreIncludeInPlayer) &&
                        asset is SubtitleAsset &&
                        (!assetId.HasValue || asset.Record.Id == assetId.Value)
                    let subtitleAsset = asset as SubtitleAsset
                    select new {
                        Type = subtitleAsset.GetType().Name,
                        Id = subtitleAsset.Record.Id,
                        Name = subtitleAsset.Name,
                        MimeType = subtitleAsset.MimeType,
                        MediaQuery = subtitleAsset.MediaQuery,
                        Language = subtitleAsset.Language,
                        MainFileUrl = selectUrl(subtitleAsset.PrivateMainFileUrl, subtitleAsset.PublicMainFileUrl)
                    };

                var assetData = new {
                    VideoAssets = videoAssetQuery.ToArray(),
                    DynamicVideoAssets = dynamicVideoAssetQuery.ToArray(),
                    ThumbnailAssets = thumbnailAssetQuery.ToArray(),
                    SubtitleAssets = subtitleAssetQuery.ToArray()
                };

                shape.AssetData = assetData;
            });
        }
    }
}