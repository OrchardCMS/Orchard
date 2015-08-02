/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />
/// <reference path="Typings/uri.d.ts" />

module Orchard.Azure.MediaServices.VideoPlayer.Injectors {

    import Data = Orchard.Azure.MediaServices.VideoPlayer.Data;

    export class Html5Injector extends Injector {

        public isSupported(): boolean {
            var videoElement: HTMLVideoElement = document.createElement("video");
            var result = videoElement && !!videoElement.canPlayType;
            this.debug("Browser supports HTML5 video: {0}", result);
            this.debug("isSupported() returns {0}.", result);

            return result;
        }

        public inject(): void {
            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();

            this.debug("Injecting player into element '{0}'.", this.container.id);

            var videoElement = $("<video controls>").attr("width", this.playerWidth).attr("height", this.playerHeight);
            if (firstThumbnailAsset)
                videoElement.attr("poster", firstThumbnailAsset.MainFileUrl);
            if (this.autoPlay)
                videoElement.attr("autoplay", "");

            var sourceElements: JQuery[] = [];

            // Adaptive streaming URLs from dynamic assets.
            _(this.assetData.DynamicVideoAssets).forEach(asset => { // Read from assetData because browser will do media query filtering.
                var smoothStreamingSourceElement = $("<source>").attr("src", asset.SmoothStreamingUrl).attr("type", "application/vnd.ms-sstr+xml");
                var hlsSourceElement = $("<source>").attr("src", asset.HlsUrl).attr("type", "application/x-mpegURL");
                var mpegDashSourceElement = $("<source>").attr("src", asset.MpegDashUrl).attr("type", "application/dash+xml");
                if (this.applyMediaQueries && asset.MediaQuery)
                    $([smoothStreamingSourceElement, hlsSourceElement, mpegDashSourceElement]).attr("media", asset.MediaQuery);
                sourceElements.push(smoothStreamingSourceElement, hlsSourceElement, mpegDashSourceElement);
            });

            // "Raw" asset video file URLs from dynamic assets (in decending bitrate order).
            _(this.assetData.DynamicVideoAssets).forEach(asset => { // Read from assetData because browser will do media query filtering.
                _((asset.EncoderMetadata && asset.EncoderMetadata.AssetFiles) || [])
                    .filter(assetFile => _(assetFile.VideoTracks).any())
                    .sort(assetFile => assetFile.Bitrate).reverse()
                    .forEach(assetFile => {
                        var url = new URI(asset.MainFileUrl).filename(assetFile.Name);
                        var sourceElement = $("<source>").attr("src", url.toString()).attr("type", assetFile.MimeType);
                        if (this.applyMediaQueries && asset.MediaQuery)
                            sourceElement.attr("media", asset.MediaQuery);
                        sourceElements.push(sourceElement);
                    });
            });

            // Asset file URLs from non-dynamic assets.
            _(this.assetData.VideoAssets).forEach(asset => { // Read from assetData because browser will do media query filtering.
                var sourceElement = $("<source>").attr("src", asset.MainFileUrl).attr("type", asset.MimeType);
                if (this.applyMediaQueries && asset.MediaQuery)
                    sourceElement.attr("media", asset.MediaQuery);
                sourceElements.push(sourceElement);
            });

            _(this.filteredAssets().SubtitleAssets).forEach(asset => {
                var sourceElement = $("<track kind=\"captions\">").attr("label", asset.Name).attr("src", asset.MainFileUrl).attr("srclang", asset.Language);
                sourceElements.push(sourceElement);
            });

            if (!_(sourceElements).any()) {
                this.debug("No sources available; cleaning up container and faulting this injector.");
                this.fault();
                return;
            }

            $(sourceElements).each((index, elem) => { $(elem).appendTo(videoElement); });
            videoElement.appendTo(this.container);

            var lastSource = <HTMLSourceElement>_(sourceElements).last()[0];

            var errorHandler = e => {
                this.debug("Error detected; cleaning up container and faulting this injector.");
                // TODO: Be a little more selective here, don't fail on any error.
                this.fault();
            };

            lastSource.addEventListener("error", errorHandler, false);
            videoElement.on("error", errorHandler);
        }

        public debug(message: string, ...args: any[]): void {
            super.debug("Html5Injector: " + message, args);
        }
    }
} 