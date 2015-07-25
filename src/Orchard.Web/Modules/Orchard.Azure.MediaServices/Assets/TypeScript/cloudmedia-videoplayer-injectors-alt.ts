/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />

module Orchard.Azure.MediaServices.VideoPlayer.Injectors {

    import Data = Orchard.Azure.MediaServices.VideoPlayer.Data;

    export class AltInjector extends Injector {

        constructor(
            container: HTMLElement,
            playerWidth: number,
            playerHeight: number,
            assetData: Data.IAssetData,
            applyMediaQueries: boolean,
            debugToConsole: boolean,
            nextInjector: Injector,
            private alternateContent: JQuery[]) { super(container, playerWidth, playerHeight, false, assetData, applyMediaQueries, debugToConsole, nextInjector); }

        public isSupported(): boolean {
            return true;
        }

        public inject(): void {
            var firstThumbnailAsset = _(this.filteredAssets().ThumbnailAssets).first();

            this.debug("Injecting alternate content into element '{0}'.", this.container.id);

            var wrapper = $("<div>")
                .addClass("cloudvideo-player-alt-wrapper")
                .css("width", this.playerWidth)
                .css("height", this.playerHeight);
            if (firstThumbnailAsset)
                wrapper.css("background-image", "url('" + firstThumbnailAsset.MainFileUrl + "')");

            var inner = $("<div>").addClass("cloudvideo-player-alt-inner").appendTo(wrapper);

            if (this.alternateContent)
                _(this.alternateContent).each(elem => { $(elem).appendTo(inner); });

            wrapper.appendTo(this.container);
        }

        public debug(message: string, ...args: any[]): void {
            super.debug("AltInjector: " + message, args);
        }
    }
} 