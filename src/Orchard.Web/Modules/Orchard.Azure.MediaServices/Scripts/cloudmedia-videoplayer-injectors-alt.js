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
                    var AltInjector = (function (_super) {
                        __extends(AltInjector, _super);
                        function AltInjector(container, playerWidth, playerHeight, assetData, applyMediaQueries, debugToConsole, nextInjector, alternateContent) {
                            _super.call(this, container, playerWidth, playerHeight, false, assetData, applyMediaQueries, debugToConsole, nextInjector);
                            this.alternateContent = alternateContent;
                        }
                        AltInjector.prototype.isSupported = function () {
                            return true;
                        };
                        AltInjector.prototype.inject = function () {
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
                                _(this.alternateContent).each(function (elem) { $(elem).appendTo(inner); });
                            wrapper.appendTo(this.container);
                        };
                        AltInjector.prototype.debug = function (message) {
                            var args = [];
                            for (var _i = 1; _i < arguments.length; _i++) {
                                args[_i - 1] = arguments[_i];
                            }
                            _super.prototype.debug.call(this, "AltInjector: " + message, args);
                        };
                        return AltInjector;
                    })(Injectors.Injector);
                    Injectors.AltInjector = AltInjector;
                })(Injectors = VideoPlayer.Injectors || (VideoPlayer.Injectors = {}));
            })(VideoPlayer = MediaServices.VideoPlayer || (MediaServices.VideoPlayer = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtdmlkZW9wbGF5ZXItaW5qZWN0b3JzLWFsdC50cyJdLCJuYW1lcyI6WyJPcmNoYXJkIiwiT3JjaGFyZC5BenVyZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllciIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkFsdEluamVjdG9yIiwiT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycy5BbHRJbmplY3Rvci5jb25zdHJ1Y3RvciIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuQWx0SW5qZWN0b3IuaXNTdXBwb3J0ZWQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuVmlkZW9QbGF5ZXIuSW5qZWN0b3JzLkFsdEluamVjdG9yLmluamVjdCIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5JbmplY3RvcnMuQWx0SW5qZWN0b3IuZGVidWciXSwibWFwcGluZ3MiOiJBQUFBLDRDQUE0QztBQUM1QyxnREFBZ0Q7Ozs7Ozs7QUFFaEQsSUFBTyxPQUFPLENBNENiO0FBNUNELFdBQU8sT0FBTztJQUFDQSxJQUFBQSxLQUFLQSxDQTRDbkJBO0lBNUNjQSxXQUFBQSxLQUFLQTtRQUFDQyxJQUFBQSxhQUFhQSxDQTRDakNBO1FBNUNvQkEsV0FBQUEsYUFBYUE7WUFBQ0MsSUFBQUEsV0FBV0EsQ0E0QzdDQTtZQTVDa0NBLFdBQUFBLFdBQVdBO2dCQUFDQyxJQUFBQSxTQUFTQSxDQTRDdkRBO2dCQTVDOENBLFdBQUFBLFNBQVNBLEVBQUNBLENBQUNBO29CQUl0REM7d0JBQWlDQywrQkFBUUE7d0JBRXJDQSxxQkFDSUEsU0FBc0JBLEVBQ3RCQSxXQUFtQkEsRUFDbkJBLFlBQW9CQSxFQUNwQkEsU0FBMEJBLEVBQzFCQSxpQkFBMEJBLEVBQzFCQSxjQUF1QkEsRUFDdkJBLFlBQXNCQSxFQUNkQSxnQkFBMEJBOzRCQUFJQyxrQkFBTUEsU0FBU0EsRUFBRUEsV0FBV0EsRUFBRUEsWUFBWUEsRUFBRUEsS0FBS0EsRUFBRUEsU0FBU0EsRUFBRUEsaUJBQWlCQSxFQUFFQSxjQUFjQSxFQUFFQSxZQUFZQSxDQUFDQSxDQUFDQTs0QkFBN0lBLHFCQUFnQkEsR0FBaEJBLGdCQUFnQkEsQ0FBVUE7d0JBQW9IQSxDQUFDQTt3QkFFcEpELGlDQUFXQSxHQUFsQkE7NEJBQ0lFLE1BQU1BLENBQUNBLElBQUlBLENBQUNBO3dCQUNoQkEsQ0FBQ0E7d0JBRU1GLDRCQUFNQSxHQUFiQTs0QkFDSUcsSUFBSUEsbUJBQW1CQSxHQUFHQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxjQUFjQSxFQUFFQSxDQUFDQSxlQUFlQSxDQUFDQSxDQUFDQSxLQUFLQSxFQUFFQSxDQUFDQTs0QkFFM0VBLElBQUlBLENBQUNBLEtBQUtBLENBQUNBLGlEQUFpREEsRUFBRUEsSUFBSUEsQ0FBQ0EsU0FBU0EsQ0FBQ0EsRUFBRUEsQ0FBQ0EsQ0FBQ0E7NEJBRWpGQSxJQUFJQSxPQUFPQSxHQUFHQSxDQUFDQSxDQUFDQSxPQUFPQSxDQUFDQTtpQ0FDbkJBLFFBQVFBLENBQUNBLCtCQUErQkEsQ0FBQ0E7aUNBQ3pDQSxHQUFHQSxDQUFDQSxPQUFPQSxFQUFFQSxJQUFJQSxDQUFDQSxXQUFXQSxDQUFDQTtpQ0FDOUJBLEdBQUdBLENBQUNBLFFBQVFBLEVBQUVBLElBQUlBLENBQUNBLFlBQVlBLENBQUNBLENBQUNBOzRCQUN0Q0EsRUFBRUEsQ0FBQ0EsQ0FBQ0EsbUJBQW1CQSxDQUFDQTtnQ0FDcEJBLE9BQU9BLENBQUNBLEdBQUdBLENBQUNBLGtCQUFrQkEsRUFBRUEsT0FBT0EsR0FBR0EsbUJBQW1CQSxDQUFDQSxXQUFXQSxHQUFHQSxJQUFJQSxDQUFDQSxDQUFDQTs0QkFFdEZBLElBQUlBLEtBQUtBLEdBQUdBLENBQUNBLENBQUNBLE9BQU9BLENBQUNBLENBQUNBLFFBQVFBLENBQUNBLDZCQUE2QkEsQ0FBQ0EsQ0FBQ0EsUUFBUUEsQ0FBQ0EsT0FBT0EsQ0FBQ0EsQ0FBQ0E7NEJBRWpGQSxFQUFFQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxnQkFBZ0JBLENBQUNBO2dDQUN0QkEsQ0FBQ0EsQ0FBQ0EsSUFBSUEsQ0FBQ0EsZ0JBQWdCQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxVQUFBQSxJQUFJQSxJQUFNQSxDQUFDQSxDQUFDQSxJQUFJQSxDQUFDQSxDQUFDQSxRQUFRQSxDQUFDQSxLQUFLQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQSxDQUFDQTs0QkFFeEVBLE9BQU9BLENBQUNBLFFBQVFBLENBQUNBLElBQUlBLENBQUNBLFNBQVNBLENBQUNBLENBQUNBO3dCQUNyQ0EsQ0FBQ0E7d0JBRU1ILDJCQUFLQSxHQUFaQSxVQUFhQSxPQUFlQTs0QkFBRUksY0FBY0E7aUNBQWRBLFdBQWNBLENBQWRBLHNCQUFjQSxDQUFkQSxJQUFjQTtnQ0FBZEEsNkJBQWNBOzs0QkFDeENBLGdCQUFLQSxDQUFDQSxLQUFLQSxZQUFDQSxlQUFlQSxHQUFHQSxPQUFPQSxFQUFFQSxJQUFJQSxDQUFDQSxDQUFDQTt3QkFDakRBLENBQUNBO3dCQUNMSixrQkFBQ0E7b0JBQURBLENBdkNBRCxBQXVDQ0MsRUF2Q2dDRCxrQkFBUUEsRUF1Q3hDQTtvQkF2Q1lBLHFCQUFXQSxjQXVDdkJBLENBQUFBO2dCQUNMQSxDQUFDQSxFQTVDOENELFNBQVNBLEdBQVRBLHFCQUFTQSxLQUFUQSxxQkFBU0EsUUE0Q3ZEQTtZQUFEQSxDQUFDQSxFQTVDa0NELFdBQVdBLEdBQVhBLHlCQUFXQSxLQUFYQSx5QkFBV0EsUUE0QzdDQTtRQUFEQSxDQUFDQSxFQTVDb0JELGFBQWFBLEdBQWJBLG1CQUFhQSxLQUFiQSxtQkFBYUEsUUE0Q2pDQTtJQUFEQSxDQUFDQSxFQTVDY0QsS0FBS0EsR0FBTEEsYUFBS0EsS0FBTEEsYUFBS0EsUUE0Q25CQTtBQUFEQSxDQUFDQSxFQTVDTSxPQUFPLEtBQVAsT0FBTyxRQTRDYiIsImZpbGUiOiJjbG91ZG1lZGlhLXZpZGVvcGxheWVyLWluamVjdG9ycy1hbHQuanMiLCJzb3VyY2VzQ29udGVudCI6WyIvLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnkuZC50c1wiIC8+XG4vLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy91bmRlcnNjb3JlLmQudHNcIiAvPlxuXG5tb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLlZpZGVvUGxheWVyLkluamVjdG9ycyB7XG5cbiAgICBpbXBvcnQgRGF0YSA9IE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5WaWRlb1BsYXllci5EYXRhO1xuXG4gICAgZXhwb3J0IGNsYXNzIEFsdEluamVjdG9yIGV4dGVuZHMgSW5qZWN0b3Ige1xuXG4gICAgICAgIGNvbnN0cnVjdG9yKFxuICAgICAgICAgICAgY29udGFpbmVyOiBIVE1MRWxlbWVudCxcbiAgICAgICAgICAgIHBsYXllcldpZHRoOiBudW1iZXIsXG4gICAgICAgICAgICBwbGF5ZXJIZWlnaHQ6IG51bWJlcixcbiAgICAgICAgICAgIGFzc2V0RGF0YTogRGF0YS5JQXNzZXREYXRhLFxuICAgICAgICAgICAgYXBwbHlNZWRpYVF1ZXJpZXM6IGJvb2xlYW4sXG4gICAgICAgICAgICBkZWJ1Z1RvQ29uc29sZTogYm9vbGVhbixcbiAgICAgICAgICAgIG5leHRJbmplY3RvcjogSW5qZWN0b3IsXG4gICAgICAgICAgICBwcml2YXRlIGFsdGVybmF0ZUNvbnRlbnQ6IEpRdWVyeVtdKSB7IHN1cGVyKGNvbnRhaW5lciwgcGxheWVyV2lkdGgsIHBsYXllckhlaWdodCwgZmFsc2UsIGFzc2V0RGF0YSwgYXBwbHlNZWRpYVF1ZXJpZXMsIGRlYnVnVG9Db25zb2xlLCBuZXh0SW5qZWN0b3IpOyB9XG5cbiAgICAgICAgcHVibGljIGlzU3VwcG9ydGVkKCk6IGJvb2xlYW4ge1xuICAgICAgICAgICAgcmV0dXJuIHRydWU7XG4gICAgICAgIH1cblxuICAgICAgICBwdWJsaWMgaW5qZWN0KCk6IHZvaWQge1xuICAgICAgICAgICAgdmFyIGZpcnN0VGh1bWJuYWlsQXNzZXQgPSBfKHRoaXMuZmlsdGVyZWRBc3NldHMoKS5UaHVtYm5haWxBc3NldHMpLmZpcnN0KCk7XG5cbiAgICAgICAgICAgIHRoaXMuZGVidWcoXCJJbmplY3RpbmcgYWx0ZXJuYXRlIGNvbnRlbnQgaW50byBlbGVtZW50ICd7MH0nLlwiLCB0aGlzLmNvbnRhaW5lci5pZCk7XG5cbiAgICAgICAgICAgIHZhciB3cmFwcGVyID0gJChcIjxkaXY+XCIpXG4gICAgICAgICAgICAgICAgLmFkZENsYXNzKFwiY2xvdWR2aWRlby1wbGF5ZXItYWx0LXdyYXBwZXJcIilcbiAgICAgICAgICAgICAgICAuY3NzKFwid2lkdGhcIiwgdGhpcy5wbGF5ZXJXaWR0aClcbiAgICAgICAgICAgICAgICAuY3NzKFwiaGVpZ2h0XCIsIHRoaXMucGxheWVySGVpZ2h0KTtcbiAgICAgICAgICAgIGlmIChmaXJzdFRodW1ibmFpbEFzc2V0KVxuICAgICAgICAgICAgICAgIHdyYXBwZXIuY3NzKFwiYmFja2dyb3VuZC1pbWFnZVwiLCBcInVybCgnXCIgKyBmaXJzdFRodW1ibmFpbEFzc2V0Lk1haW5GaWxlVXJsICsgXCInKVwiKTtcblxuICAgICAgICAgICAgdmFyIGlubmVyID0gJChcIjxkaXY+XCIpLmFkZENsYXNzKFwiY2xvdWR2aWRlby1wbGF5ZXItYWx0LWlubmVyXCIpLmFwcGVuZFRvKHdyYXBwZXIpO1xuXG4gICAgICAgICAgICBpZiAodGhpcy5hbHRlcm5hdGVDb250ZW50KVxuICAgICAgICAgICAgICAgIF8odGhpcy5hbHRlcm5hdGVDb250ZW50KS5lYWNoKGVsZW0gPT4geyAkKGVsZW0pLmFwcGVuZFRvKGlubmVyKTsgfSk7XG5cbiAgICAgICAgICAgIHdyYXBwZXIuYXBwZW5kVG8odGhpcy5jb250YWluZXIpO1xuICAgICAgICB9XG5cbiAgICAgICAgcHVibGljIGRlYnVnKG1lc3NhZ2U6IHN0cmluZywgLi4uYXJnczogYW55W10pOiB2b2lkIHtcbiAgICAgICAgICAgIHN1cGVyLmRlYnVnKFwiQWx0SW5qZWN0b3I6IFwiICsgbWVzc2FnZSwgYXJncyk7XG4gICAgICAgIH1cbiAgICB9XG59ICJdLCJzb3VyY2VSb290IjoiL3NvdXJjZS8ifQ==