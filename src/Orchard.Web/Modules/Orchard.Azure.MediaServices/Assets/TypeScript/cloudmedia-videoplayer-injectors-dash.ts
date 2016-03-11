/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />

module Orchard.Azure.MediaServices.VideoPlayer.Injectors {

    import Data = Orchard.Azure.MediaServices.VideoPlayer.Data;

    declare var Dash: any;
    declare var MediaPlayer: any;

    interface PlayerErrorEventArgs {
        type: string;
        error: string;
        event: {
            id?: string;
            message?: string;
            request?: XMLHttpRequest;
            manifest?: any;
        }
    }

    export class DashInjector extends Injector {

        public isSupported(): boolean {
            var videoElement: HTMLVideoElement = document.createElement("video");

            var hasH264 = videoElement && videoElement.canPlayType && !!videoElement.canPlayType("video/mp4; codecs=\"avc1.42001E, mp4a.40.2\"");
            var hasMse = MediaSource && MediaSource.isTypeSupported && MediaSource.isTypeSupported("video/mp4; codecs=\"avc1.4d404f\"");
            var hasDynamicAssets = _(this.filteredAssets().DynamicVideoAssets).any();

            this.debug("Browser supports HTML5 video and the H264 and AAC codecs: {0}", hasH264);
            this.debug("Browser supports the Media Source Extensions API: {0}", hasMse);
            this.debug("Item has at least one dynamic video asset: {0}", hasDynamicAssets);

            var result = hasH264 && hasMse && hasDynamicAssets;
            this.debug("isSupported() returns {0}.", result);

            return result;
        }

        public inject(): void {
            var firstDynamicAsset = _(this.filteredAssets().DynamicVideoAssets).first();
            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();

            this.debug("Injecting player into element '{0}'.", this.container.id);

            var videoElement = $("<video controls>").attr("width", this.playerWidth).attr("height", this.playerHeight);
            if (firstThumbnailAsset)
                videoElement.attr("poster", firstThumbnailAsset.MainFileUrl);
            videoElement.appendTo(this.container);

            var url = firstDynamicAsset.MpegDashUrl;
            var context = new Dash.di.DashContext();
            var player = new MediaPlayer(context);
            player.startup();

            player.addEventListener("error", (e: PlayerErrorEventArgs) => {
                this.debug("Error of type '{0}' detected; cleaning up container and faulting this injector.", e.error);
                // TODO: Be a little more selective here, don't fail on any error.
                this.fault();
            });

            player.debug.setLogToBrowserConsole(false);
            player.attachView(videoElement[0]);
            player.attachSource(url);
            player.setAutoPlay(this.autoPlay);
        }

        public debug(message: string, ...args: any[]): void {
            super.debug("DashInjector: " + message, args);
        }
    }
} 