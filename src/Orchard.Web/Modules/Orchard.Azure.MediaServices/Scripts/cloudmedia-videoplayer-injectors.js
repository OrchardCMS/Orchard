var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var VideoPlayer;
            (function (VideoPlayer) {
                var Injectors;
                (function (Injectors) {
                    var Injector = (function () {
                        function Injector(container, playerWidth, playerHeight, autoPlay, assetData, applyMediaQueries, debugToConsole, nextInjector) {
                            this.container = container;
                            this.playerWidth = playerWidth;
                            this.playerHeight = playerHeight;
                            this.autoPlay = autoPlay;
                            this.assetData = assetData;
                            this.applyMediaQueries = applyMediaQueries;
                            this.debugToConsole = debugToConsole;
                            this.nextInjector = nextInjector;
                            this._isFaulted = false;
                        }
                        Injector.prototype.isFaulted = function () {
                            return this._isFaulted;
                        };
                        Injector.prototype.invoke = function () {
                            if (this.isSupported())
                                this.inject();
                            else if (this.nextInjector)
                                this.nextInjector.invoke();
                        };
                        Injector.prototype.isSupported = function () {
                            throw new Error("This method is abstract and must be overridden in an inherited class.");
                        };
                        Injector.prototype.inject = function () {
                            throw new Error("This method is abstract and must be overridden in an inherited class.");
                        };
                        Injector.prototype.filteredAssets = function () {
                            if (!this.applyMediaQueries)
                                return this.assetData;
                            var hasMatchingMediaQuery = function (asset) {
                                return !asset.MediaQuery || window.matchMedia(asset.MediaQuery).matches;
                            };
                            return {
                                VideoAssets: _(this.assetData.VideoAssets).filter(hasMatchingMediaQuery),
                                DynamicVideoAssets: _(this.assetData.DynamicVideoAssets).filter(hasMatchingMediaQuery),
                                ThumbnailAssets: _(this.assetData.ThumbnailAssets).filter(hasMatchingMediaQuery),
                                SubtitleAssets: _(this.assetData.SubtitleAssets).filter(hasMatchingMediaQuery)
                            };
                        };
                        Injector.prototype.fault = function () {
                            if (!this._isFaulted) {
                                this._isFaulted = true;
                                $(this.container).empty();
                                if (this.nextInjector)
                                    this.nextInjector.invoke();
                            }
                        };
                        Injector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            if (this.debugToConsole) {
                                console.debug(message.replace(/{(\d+)}/g, function (match, index) { return (typeof args[index] != "undefined" ? args[index] : match); }));
                            }
                        };
                        return Injector;
                    })();
                    Injectors.Injector = Injector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLnRzIl0sIm5hbWVzIjpbIk9yY2hhcmQiLCJPcmNoYXJkLkF6dXJlIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuSW5qZWN0b3IiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkluamVjdG9yLmNvbnN0cnVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5JbmplY3Rvci5pc0ZhdWx0ZWQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkluamVjdG9yLmludm9rZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuSW5qZWN0b3IuaXNTdXBwb3J0ZWQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkluamVjdG9yLmluamVjdCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuSW5qZWN0b3IuZmlsdGVyZWRBc3NldHMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkluamVjdG9yLmZhdWx0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5JbmplY3Rvci5kZWJ1ZyJdLCJtYXBwaW5ncyI6IkFBQUEsSUFBTyxPQUFPLENBbUViO0FBbkVELFdBQU8sT0FBTztJQUFD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QUFUQSxxQkFBU0EsS0FBVEEscUJBQVNBLFFBbUV2REE7WUFBREEsQ0FBQ0EsRUFuRWtDRCxXQUFXQSxHQUFYQSx5QkFBV0EsS0FBWEEseUJBQVdBLFFBbUU3Q0E7UUFBREEsQ0FBQ0EsRUFuRW9CRCxhQUFhQSxHQUFiQSxtQkFBYUEsS0FBYkEsbUJBQWFBLFFBbUVqQ0E7SUFBREEsQ0FBQ0EsRUFuRWNELEtBQUtBLEdBQUxBLGFBQUtBLEtBQUxBLGFBQUtBLFFBbUVuQkE7QUFBREEsQ0FBQ0EsRUFuRU0sT0FBTyxLQUFQLE9BQU8sUUFtRWIiLCJmaWxlIjoiY2xvdWRtZWRpYS12aWRlb3BsYXllci1pbmplY3RvcnMuanMiLCJzb3VyY2VzQ29udGVudCI6WyJtb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycyB7XHJcblxyXG4gICAgaW1wb3J0IERhdGEgPSBPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuRGF0YTtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgSW5qZWN0b3Ige1xyXG5cclxuICAgICAgICBjb25zdHJ1Y3RvcihcclxuICAgICAgICAgICAgcHVibGljIGNvbnRhaW5lcjogSFRNTEVsZW1lbnQsXHJcbiAgICAgICAgICAgIHB1YmxpYyBwbGF5ZXJXaWR0aDogbnVtYmVyLFxyXG4gICAgICAgICAgICBwdWJsaWMgcGxheWVySGVpZ2h0OiBudW1iZXIsXHJcbiAgICAgICAgICAgIHB1YmxpYyBhdXRvUGxheTogYm9vbGVhbixcclxuICAgICAgICAgICAgcHVibGljIGFzc2V0RGF0YTogRGF0YS5JQXNzZXREYXRhLFxyXG4gICAgICAgICAgICBwdWJsaWMgYXBwbHlNZWRpYVF1ZXJpZXM6IGJvb2xlYW4sXHJcbiAgICAgICAgICAgIHByaXZhdGUgZGVidWdUb0NvbnNvbGU6IGJvb2xlYW4sXHJcbiAgICAgICAgICAgIHByaXZhdGUgbmV4dEluamVjdG9yOiBJbmplY3RvcikgeyB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgX2lzRmF1bHRlZDogYm9vbGVhbiA9IGZhbHNlO1xyXG4gICAgICAgIHB1YmxpYyBpc0ZhdWx0ZWQoKTogYm9vbGVhbiB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLl9pc0ZhdWx0ZWQ7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgaW52b2tlKCkge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5pc1N1cHBvcnRlZCgpKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5pbmplY3QoKTtcclxuICAgICAgICAgICAgZWxzZSBpZiAodGhpcy5uZXh0SW5qZWN0b3IpXHJcbiAgICAgICAgICAgICAgICB0aGlzLm5leHRJbmplY3Rvci5pbnZva2UoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHB1YmxpYyBpc1N1cHBvcnRlZCgpOiBib29sZWFuIHtcclxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiVGhpcyBtZXRob2QgaXMgYWJzdHJhY3QgYW5kIG11c3QgYmUgb3ZlcnJpZGRlbiBpbiBhbiBpbmhlcml0ZWQgY2xhc3MuXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHVibGljIGluamVjdCgpOiB2b2lkIHtcclxuICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiVGhpcyBtZXRob2QgaXMgYWJzdHJhY3QgYW5kIG11c3QgYmUgb3ZlcnJpZGRlbiBpbiBhbiBpbmhlcml0ZWQgY2xhc3MuXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHVibGljIGZpbHRlcmVkQXNzZXRzKCk6IERhdGEuSUFzc2V0RGF0YSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5hcHBseU1lZGlhUXVlcmllcylcclxuICAgICAgICAgICAgICAgIHJldHVybiB0aGlzLmFzc2V0RGF0YTtcclxuXHJcbiAgICAgICAgICAgIHZhciBoYXNNYXRjaGluZ01lZGlhUXVlcnkgPSBmdW5jdGlvbiAoYXNzZXQ6IERhdGEuSUFzc2V0KSB7XHJcbiAgICAgICAgICAgICAgICByZXR1cm4gIWFzc2V0Lk1lZGlhUXVlcnkgfHwgd2luZG93Lm1hdGNoTWVkaWEoYXNzZXQuTWVkaWFRdWVyeSkubWF0Y2hlcztcclxuICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiB7XHJcbiAgICAgICAgICAgICAgICBWaWRlb0Fzc2V0czogXyh0aGlzLmFzc2V0RGF0YS5WaWRlb0Fzc2V0cykuZmlsdGVyKGhhc01hdGNoaW5nTWVkaWFRdWVyeSksXHJcbiAgICAgICAgICAgICAgICBEeW5hbWljVmlkZW9Bc3NldHM6IF8odGhpcy5hc3NldERhdGEuRHluYW1pY1ZpZGVvQXNzZXRzKS5maWx0ZXIoaGFzTWF0Y2hpbmdNZWRpYVF1ZXJ5KSxcclxuICAgICAgICAgICAgICAgIFRodW1ibmFpbEFzc2V0czogXyh0aGlzLmFzc2V0RGF0YS5UaHVtYm5haWxBc3NldHMpLmZpbHRlcihoYXNNYXRjaGluZ01lZGlhUXVlcnkpLFxyXG4gICAgICAgICAgICAgICAgU3VidGl0bGVBc3NldHM6IF8odGhpcy5hc3NldERhdGEuU3VidGl0bGVBc3NldHMpLmZpbHRlcihoYXNNYXRjaGluZ01lZGlhUXVlcnkpXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwdWJsaWMgZmF1bHQoKSB7XHJcbiAgICAgICAgICAgIGlmICghdGhpcy5faXNGYXVsdGVkKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl9pc0ZhdWx0ZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgJCh0aGlzLmNvbnRhaW5lcikuZW1wdHkoKTtcclxuICAgICAgICAgICAgICAgIGlmICh0aGlzLm5leHRJbmplY3RvcilcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLm5leHRJbmplY3Rvci5pbnZva2UoKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgICAgICAgXHJcbiAgICAgICAgcHVibGljIGRlYnVnKG1lc3NhZ2U6IHN0cmluZywgLi4uYXJnczogYW55W10pOiB2b2lkIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMuZGVidWdUb0NvbnNvbGUpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUuZGVidWcoKDxhbnk+bWVzc2FnZSkucmVwbGFjZSgveyhcXGQrKX0vZywgKG1hdGNoOiBzdHJpbmcsIGluZGV4OiBudW1iZXIpID0+IHsgcmV0dXJuICh0eXBlb2YgYXJnc1tpbmRleF0gIT0gXCJ1bmRlZmluZWRcIiA/IGFyZ3NbaW5kZXhdIDogbWF0Y2gpOyB9KSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcbn0gIl0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9