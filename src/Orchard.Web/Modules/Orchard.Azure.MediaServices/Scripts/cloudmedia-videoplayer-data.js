var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var VideoPlayer;
            (function (VideoPlayer) {
                var Data;
                (function (Data) {
                    (function (AssetType) {
                        AssetType[AssetType["VideoAsset"] = 0] = "VideoAsset";
                        AssetType[AssetType["DynamicVideoAsset"] = 1] = "DynamicVideoAsset";
                        AssetType[AssetType["ThumbnailAsset"] = 2] = "ThumbnailAsset";
                        AssetType[AssetType["SubtitleAsset"] = 3] = "SubtitleAsset";
                    })(Data.AssetType || (Data.AssetType = {}));
                    var AssetType = Data.AssetType;
                })(Data = VideoPlayer.Data || (VideoPlayer.Data = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
