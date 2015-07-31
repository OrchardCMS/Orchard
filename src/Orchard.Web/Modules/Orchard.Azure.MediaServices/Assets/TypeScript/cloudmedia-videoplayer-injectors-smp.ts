/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />

module Orchard.Azure.MediaServices.VideoPlayer.Injectors {

    import Data = Orchard.Azure.MediaServices.VideoPlayer.Data;

    declare var swfobject: any;

    export var instances: SmpInjector[] = new Array();

    export class SmpInjector extends Injector {

        private flashVersion = "10.2.0";
        private innerElementId: string;

        constructor(
            container: HTMLElement,
            playerWidth: number,
            playerHeight: number,
            autoPlay: boolean,
            assetData: Data.IAssetData,
            applyMediaQueries: boolean,
            debugToConsole: boolean,
            nextInjector: Injector,
            private contentBaseUrl: string) {
            super(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, debugToConsole, nextInjector);
            this.innerElementId = container.id + "-inner";
        }

        public isSupported(): boolean {
            var browserHasFlash = swfobject.hasFlashPlayerVersion(this.flashVersion);
            var hasDynamicAssets = _(this.filteredAssets().DynamicVideoAssets).any();
            var result = browserHasFlash && hasDynamicAssets;

            this.debug("Browser has required Flash version: {0}", browserHasFlash);
            this.debug("Item has at least one dynamic video asset: {0}", hasDynamicAssets);

            this.debug("isSupported() returns {0}.", result);
            return result;
        }

        public inject(): void {
            var firstDynamicAsset = _(this.filteredAssets().DynamicVideoAssets).first();
            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();

            var flashvars = {
                src: firstDynamicAsset.SmoothStreamingUrl,
                plugin_AdaptiveStreamingPlugin: encodeURIComponent(this.contentBaseUrl + "MSAdaptiveStreamingPlugin.swf"),
                AdaptiveStreamingPlugin_retryLive: "true",
                AdaptiveStreamingPlugin_retryInterval: "10",
                autoPlay: this.autoPlay.toString(),
                bufferingOverlay: "false",
                poster: firstThumbnailAsset ? encodeURIComponent(firstThumbnailAsset.MainFileUrl) : null,
                javascriptCallbackFunction: "Orchard.Azure.MediaServices.VideoPlayer.Injectors.onSmpBridgeCreated"
            };
             
            var params = {
                allowFullScreen: "true",
                allowScriptAccess: "always",
                wmode: "direct"
            };

            var attributes = {
                id: this.innerElementId
            };

            $("<div>").attr("id", this.innerElementId).appendTo(this.container);
            this.debug("Injecting player into element '{0}'.", this.container.id);

            swfobject.embedSWF(
                this.contentBaseUrl + "StrobeMediaPlayback.swf",
                this.innerElementId,
                this.playerWidth.toString(),
                this.playerHeight.toString(),
                this.flashVersion,
                this.contentBaseUrl + "expressInstall.swf",
                flashvars,
                params,
                attributes,
                e => {
                    if (!e.success)
                        this.fault();
                });

            instances[this.innerElementId] = this;
        }
         
        public onMediaPlayerStateChange(state: string) {
            if (state == "playbackError") {
                this.debug("Playback error detected; cleaning up container and faulting this injector.");
                instances[this.innerElementId] = null;
                this.fault();
            }
        }

        public debug(message: string, ...args: any[]): void {
            super.debug("SmpInjector: " + message, args);
        }
    }
     
    export function onSmpBridgeCreated(playerElementId: string) {
        var player = document.getElementById(playerElementId);
        if (player) {
            (<any>player).addEventListener("mediaPlayerStateChange", "Orchard.Azure.MediaServices.VideoPlayer.Injectors.instances[\"" + playerElementId + "\"].onMediaPlayerStateChange");
        }
    } 
}