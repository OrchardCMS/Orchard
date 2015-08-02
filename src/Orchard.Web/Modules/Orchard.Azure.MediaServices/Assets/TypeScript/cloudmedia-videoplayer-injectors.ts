module Orchard.Azure.MediaServices.VideoPlayer.Injectors {

    import Data = Orchard.Azure.MediaServices.VideoPlayer.Data;

    export class Injector {

        constructor(
            public container: HTMLElement,
            public playerWidth: number,
            public playerHeight: number,
            public autoPlay: boolean,
            public assetData: Data.IAssetData,
            public applyMediaQueries: boolean,
            private debugToConsole: boolean,
            private nextInjector: Injector) { }

        private _isFaulted: boolean = false;
        public isFaulted(): boolean {
            return this._isFaulted;
        }

        public invoke() {
            if (this.isSupported())
                this.inject();
            else if (this.nextInjector)
                this.nextInjector.invoke();
        }

        public isSupported(): boolean {
            throw new Error("This method is abstract and must be overridden in an inherited class.");
        }

        public inject(): void {
            throw new Error("This method is abstract and must be overridden in an inherited class.");
        }

        public filteredAssets(): Data.IAssetData {
            if (!this.applyMediaQueries)
                return this.assetData;

            var hasMatchingMediaQuery = function (asset: Data.IAsset) {
                return !asset.MediaQuery || window.matchMedia(asset.MediaQuery).matches;
            };

            return {
                VideoAssets: _(this.assetData.VideoAssets).filter(hasMatchingMediaQuery),
                DynamicVideoAssets: _(this.assetData.DynamicVideoAssets).filter(hasMatchingMediaQuery),
                ThumbnailAssets: _(this.assetData.ThumbnailAssets).filter(hasMatchingMediaQuery),
                SubtitleAssets: _(this.assetData.SubtitleAssets).filter(hasMatchingMediaQuery)
            };
        }

        public fault() {
            if (!this._isFaulted) {
                this._isFaulted = true;
                $(this.container).empty();
                if (this.nextInjector)
                    this.nextInjector.invoke();
            }
        }
         
        public debug(message: string, ...args: any[]): void {
            if (this.debugToConsole) {
                console.debug((<any>message).replace(/{(\d+)}/g, (match: string, index: number) => { return (typeof args[index] != "undefined" ? args[index] : match); }));
            }
        }
    }
} 