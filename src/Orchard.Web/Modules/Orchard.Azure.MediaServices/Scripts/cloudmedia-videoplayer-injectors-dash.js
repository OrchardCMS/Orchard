/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/underscore.d.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
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
                    var DashInjector = (function (_super) {
                        __extends(DashInjector, _super);
                        function DashInjector() {
                            _super.apply(this, arguments);
                        }
                        DashInjector.prototype.isSupported = function () {
                            var videoElement = document.createElement("video");
                            var hasH264 = videoElement && videoElement.canPlayType && !!videoElement.canPlayType("video/mp4; codecs=\"avc1.42001E, mp4a.40.2\"");
                            var hasMse = MediaSource && MediaSource.isTypeSupported && MediaSource.isTypeSupported("video/mp4; codecs=\"avc1.4d404f\"");
                            var hasDynamicAssets = _(this.filteredAssets().DynamicVideoAssets).any();
                            this.debug("Browser supports HTML5 video and the H264 and AAC codecs: {0}", hasH264);
                            this.debug("Browser supports the Media Source Extensions API: {0}", hasMse);
                            this.debug("Item has at least one dynamic video asset: {0}", hasDynamicAssets);
                            var result = hasH264 && hasMse && hasDynamicAssets;
                            this.debug("isSupported() returns {0}.", result);
                            return result;
                        };
                        DashInjector.prototype.inject = function () {
                            var _this = this;
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
                            player.addEventListener("error", function (e) {
                                _this.debug("Error of type '{0}' detected; cleaning up container and faulting this injector.", e.error);
                                // TODO: Be a little more selective here, don't fail on any error.
                                _this.fault();
                            });
                            player.debug.setLogToBrowserConsole(false);
                            player.attachView(videoElement[0]);
                            player.attachSource(url);
                            player.setAutoPlay(this.autoPlay);
                        };
                        DashInjector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            _super.prototype.debug.call(this, "DashInjector: " + message, args);
                        };
                        return DashInjector;
                    })(Injectors.Injector);
                    Injectors.DashInjector = DashInjector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLWRhc2gudHMiXSwibmFtZXMiOlsiT3JjaGFyZCIsIk9yY2hhcmQuQXp1cmUiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5EYXNoSW5qZWN0b3IiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkRhc2hJbmplY3Rvci5jb25zdHJ1Y3RvciIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuRGFzaEluamVjdG9yLmlzU3VwcG9ydGVkIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5EYXNoSW5qZWN0b3IuaW5qZWN0IiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5EYXNoSW5qZWN0b3IuZGVidWciXSwibWFwcGluZ3MiOiJBQUFBLDRDQUE0QztBQUM1QyxnREFBZ0Q7Ozs7Ozs7QUFFaEQsSUFBTyxPQUFPLENBcUViO0FBckVELFdBQU8sT0FBTztJQUFDQSxJQUFBQSxLQUFLQSxDQXFFbkJBO0lBckVjQSxXQUFBQSxLQUFLQTtRQUFDQyxJQUFBQSxhQUFhQSxDQXFFakNBO1FBckVvQkEsV0FBQUEsYUFBYUE7WUFBQ0MsSUFBQUEsV0FBV0EsQ0FxRTdDQTtZQXJFa0NBLFdBQUFBLFdBQVdBO2dCQUFDQyxJQUFBQSxTQUFTQSxDQXFFdkRBO2dCQXJFOENBLFdBQUFBLFNBQVNBLEVBQUNBLENBQUNBO29CQWtCdERDO3dCQUFrQ0MsZ0NBQVFBO3dCQUExQ0E7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QUFMQSxhQUFLQSxRQXFFbkJBO0FBQURBLENBQUNBLEVBckVNLE9BQU8sS0FBUCxPQUFPLFFBcUViIiwiZmlsZSI6ImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLWRhc2guanMiLCJzb3VyY2VzQ29udGVudCI6WyIvLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnkuZC50c1wiIC8+XG4vLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy91bmRlcnNjb3JlLmQudHNcIiAvPlxuXG5tb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycyB7XG5cbiAgICBpbXBvcnQgRGF0YSA9IE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5EYXRhO1xuXG4gICAgZGVjbGFyZSB2YXIgRGFzaDogYW55O1xuICAgIGRlY2xhcmUgdmFyIE1lZGlhUGxheWVyOiBhbnk7XG5cbiAgICBpbnRlcmZhY2UgUGxheWVyRXJyb3JFdmVudEFyZ3Mge1xuICAgICAgICB0eXBlOiBzdHJpbmc7XG4gICAgICAgIGVycm9yOiBzdHJpbmc7XG4gICAgICAgIGV2ZW50OiB7XG4gICAgICAgICAgICBpZD86IHN0cmluZztcbiAgICAgICAgICAgIG1lc3NhZ2U/OiBzdHJpbmc7XG4gICAgICAgICAgICByZXF1ZXN0PzogWE1MSHR0cFJlcXVlc3Q7XG4gICAgICAgICAgICBtYW5pZmVzdD86IGFueTtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGV4cG9ydCBjbGFzcyBEYXNoSW5qZWN0b3IgZXh0ZW5kcyBJbmplY3RvciB7XG5cbiAgICAgICAgcHVibGljIGlzU3VwcG9ydGVkKCk6IGJvb2xlYW4ge1xuICAgICAgICAgICAgdmFyIHZpZGVvRWxlbWVudDogSFRNTFZpZGVvRWxlbWVudCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ2aWRlb1wiKTtcblxuICAgICAgICAgICAgdmFyIGhhc0gyNjQgPSB2aWRlb0VsZW1lbnQgJiYgdmlkZW9FbGVtZW50LmNhblBsYXlUeXBlICYmICEhdmlkZW9FbGVtZW50LmNhblBsYXlUeXBlKFwidmlkZW8vbXA0OyBjb2RlY3M9XFxcImF2YzEuNDIwMDFFLCBtcDRhLjQwLjJcXFwiXCIpO1xuICAgICAgICAgICAgdmFyIGhhc01zZSA9IE1lZGlhU291cmNlICYmIE1lZGlhU291cmNlLmlzVHlwZVN1cHBvcnRlZCAmJiBNZWRpYVNvdXJjZS5pc1R5cGVTdXBwb3J0ZWQoXCJ2aWRlby9tcDQ7IGNvZGVjcz1cXFwiYXZjMS40ZDQwNGZcXFwiXCIpO1xuICAgICAgICAgICAgdmFyIGhhc0R5bmFtaWNBc3NldHMgPSBfKHRoaXMuZmlsdGVyZWRBc3NldHMoKS5EeW5hbWljVmlkZW9Bc3NldHMpLmFueSgpO1xuXG4gICAgICAgICAgICB0aGlzLmRlYnVnKFwiQnJvd3NlciBzdXBwb3J0cyBIVE1MNSB2aWRlbyBhbmQgdGhlIEgyNjQgYW5kIEFBQyBjb2RlY3M6IHswfVwiLCBoYXNIMjY0KTtcbiAgICAgICAgICAgIHRoaXMuZGVidWcoXCJCcm93c2VyIHN1cHBvcnRzIHRoZSBNZWRpYSBTb3VyY2UgRXh0ZW5zaW9ucyBBUEk6IHswfVwiLCBoYXNNc2UpO1xuICAgICAgICAgICAgdGhpcy5kZWJ1ZyhcIkl0ZW0gaGFzIGF0IGxlYXN0IG9uZSBkeW5hbWljIHZpZGVvIGFzc2V0OiB7MH1cIiwgaGFzRHluYW1pY0Fzc2V0cyk7XG5cbiAgICAgICAgICAgIHZhciByZXN1bHQgPSBoYXNIMjY0ICYmIGhhc01zZSAmJiBoYXNEeW5hbWljQXNzZXRzO1xuICAgICAgICAgICAgdGhpcy5kZWJ1ZyhcImlzU3VwcG9ydGVkKCkgcmV0dXJucyB7MH0uXCIsIHJlc3VsdCk7XG5cbiAgICAgICAgICAgIHJldHVybiByZXN1bHQ7XG4gICAgICAgIH1cblxuICAgICAgICBwdWJsaWMgaW5qZWN0KCk6IHZvaWQge1xuICAgICAgICAgICAgdmFyIGZpcnN0RHluYW1pY0Fzc2V0ID0gXyh0aGlzLmZpbHRlcmVkQXNzZXRzKCkuRHluYW1pY1ZpZGVvQXNzZXRzKS5maXJzdCgpO1xuICAgICAgICAgICAgdmFyIGZpcnN0VGh1bWJuYWlsQXNzZXQgPSBfKHRoaXMuZmlsdGVyZWRBc3NldHMoKS5UaHVtYm5haWxBc3NldHMpLmZpcnN0KCk7XG5cbiAgICAgICAgICAgIHRoaXMuZGVidWcoXCJJbmplY3RpbmcgcGxheWVyIGludG8gZWxlbWVudCAnezB9Jy5cIiwgdGhpcy5jb250YWluZXIuaWQpO1xuXG4gICAgICAgICAgICB2YXIgdmlkZW9FbGVtZW50ID0gJChcIjx2aWRlbyBjb250cm9scz5cIikuYXR0cihcIndpZHRoXCIsIHRoaXMucGxheWVyV2lkdGgpLmF0dHIoXCJoZWlnaHRcIiwgdGhpcy5wbGF5ZXJIZWlnaHQpO1xuICAgICAgICAgICAgaWYgKGZpcnN0VGh1bWJuYWlsQXNzZXQpXG4gICAgICAgICAgICAgICAgdmlkZW9FbGVtZW50LmF0dHIoXCJwb3N0ZXJcIiwgZmlyc3RUaHVtYm5haWxBc3NldC5NYWluRmlsZVVybCk7XG4gICAgICAgICAgICB2aWRlb0VsZW1lbnQuYXBwZW5kVG8odGhpcy5jb250YWluZXIpO1xuXG4gICAgICAgICAgICB2YXIgdXJsID0gZmlyc3REeW5hbWljQXNzZXQuTXBlZ0Rhc2hVcmw7XG4gICAgICAgICAgICB2YXIgY29udGV4dCA9IG5ldyBEYXNoLmRpLkRhc2hDb250ZXh0KCk7XG4gICAgICAgICAgICB2YXIgcGxheWVyID0gbmV3IE1lZGlhUGxheWVyKGNvbnRleHQpO1xuICAgICAgICAgICAgcGxheWVyLnN0YXJ0dXAoKTtcblxuICAgICAgICAgICAgcGxheWVyLmFkZEV2ZW50TGlzdGVuZXIoXCJlcnJvclwiLCAoZTogUGxheWVyRXJyb3JFdmVudEFyZ3MpID0+IHtcbiAgICAgICAgICAgICAgICB0aGlzLmRlYnVnKFwiRXJyb3Igb2YgdHlwZSAnezB9JyBkZXRlY3RlZDsgY2xlYW5pbmcgdXAgY29udGFpbmVyIGFuZCBmYXVsdGluZyB0aGlzIGluamVjdG9yLlwiLCBlLmVycm9yKTtcbiAgICAgICAgICAgICAgICAvLyBUT0RPOiBCZSBhIGxpdHRsZSBtb3JlIHNlbGVjdGl2ZSBoZXJlLCBkb24ndCBmYWlsIG9uIGFueSBlcnJvci5cbiAgICAgICAgICAgICAgICB0aGlzLmZhdWx0KCk7XG4gICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgcGxheWVyLmRlYnVnLnNldExvZ1RvQnJvd3NlckNvbnNvbGUoZmFsc2UpO1xuICAgICAgICAgICAgcGxheWVyLmF0dGFjaFZpZXcodmlkZW9FbGVtZW50WzBdKTtcbiAgICAgICAgICAgIHBsYXllci5hdHRhY2hTb3VyY2UodXJsKTtcbiAgICAgICAgICAgIHBsYXllci5zZXRBdXRvUGxheSh0aGlzLmF1dG9QbGF5KTtcbiAgICAgICAgfVxuXG4gICAgICAgIHB1YmxpYyBkZWJ1ZyhtZXNzYWdlOiBzdHJpbmcsIC4uLmFyZ3M6IGFueVtdKTogdm9pZCB7XG4gICAgICAgICAgICBzdXBlci5kZWJ1ZyhcIkRhc2hJbmplY3RvcjogXCIgKyBtZXNzYWdlLCBhcmdzKTtcbiAgICAgICAgfVxuICAgIH1cbn0gIl0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9