/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var AssetEdit;
            (function (AssetEdit) {
                $(function () {
                    var localStorage = window["localStorage"];
                    $("#tabs").tabs({
                        activate: function () {
                            if (localStorage && localStorage.setItem)
                                localStorage.setItem("selectedAssetTab", $("#tabs").tabs("option", "active"));
                        },
                        active: localStorage && localStorage.getItem ? localStorage.getItem("selectedAssetTab") : null
                    }).show();
                });
            })(AssetEdit = MediaServices.AssetEdit || (MediaServices.AssetEdit = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1hc3NldC50cyJdLCJuYW1lcyI6WyJPcmNoYXJkIiwiT3JjaGFyZC5BenVyZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5Bc3NldEVkaXQiXSwibWFwcGluZ3MiOiJBQUFBLDRDQUE0QztBQUM1Qyw4Q0FBOEM7QUFFOUMsSUFBTyxPQUFPLENBV2I7QUFYRCxXQUFPLE9BQU87SUFBQ0EsSUFBQUEsS0FBS0EsQ0FXbkJBO0lBWGNBLFdBQUFBLEtBQUtBO1FBQUNDLElBQUFBLGFBQWFBLENBV2pDQTtRQVhvQkEsV0FBQUEsYUFBYUE7WUFBQ0MsSUFBQUEsU0FBU0EsQ0FXM0NBO1lBWGtDQSxXQUFBQSxTQUFTQSxFQUFDQSxDQUFDQTtnQkFDMUNDLENBQUNBLENBQUNBO29CQUNFLElBQUksWUFBWSxHQUFHLE1BQU0sQ0FBQyxjQUFjLENBQUMsQ0FBQztvQkFDMUMsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQzt3QkFDWixRQUFRLEVBQUU7NEJBQ04sRUFBRSxDQUFDLENBQUMsWUFBWSxJQUFJLFlBQVksQ0FBQyxPQUFPLENBQUM7Z0NBQ3JDLFlBQVksQ0FBQyxPQUFPLENBQUMsa0JBQWtCLEVBQUUsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxDQUFDLElBQUksQ0FBQyxRQUFRLEVBQUUsUUFBUSxDQUFDLENBQUMsQ0FBQzt3QkFDdEYsQ0FBQzt3QkFDRCxNQUFNLEVBQUUsWUFBWSxJQUFJLFlBQVksQ0FBQyxPQUFPLEdBQUcsWUFBWSxDQUFDLE9BQU8sQ0FBQyxrQkFBa0IsQ0FBQyxHQUFHLElBQUk7cUJBQ2pHLENBQUMsQ0FBQyxJQUFJLEVBQUUsQ0FBQztnQkFDZCxDQUFDLENBQUNBLENBQUNBO1lBQ1BBLENBQUNBLEVBWGtDRCxTQUFTQSxHQUFUQSx1QkFBU0EsS0FBVEEsdUJBQVNBLFFBVzNDQTtRQUFEQSxDQUFDQSxFQVhvQkQsYUFBYUEsR0FBYkEsbUJBQWFBLEtBQWJBLG1CQUFhQSxRQVdqQ0E7SUFBREEsQ0FBQ0EsRUFYY0QsS0FBS0EsR0FBTEEsYUFBS0EsS0FBTEEsYUFBS0EsUUFXbkJBO0FBQURBLENBQUNBLEVBWE0sT0FBTyxLQUFQLE9BQU8sUUFXYiIsImZpbGUiOiJjbG91ZG1lZGlhLWVkaXQtYXNzZXQuanMiLCJzb3VyY2VzQ29udGVudCI6WyIvLy8gPHJlZmVyZW5jZSBwYXRoPVwiVHlwaW5ncy9qcXVlcnkuZC50c1wiIC8+XHJcbi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeXVpLmQudHNcIiAvPlxyXG5cclxubW9kdWxlIE9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5Bc3NldEVkaXQge1xyXG4gICAgJChmdW5jdGlvbigpIHtcclxuICAgICAgICB2YXIgbG9jYWxTdG9yYWdlID0gd2luZG93W1wibG9jYWxTdG9yYWdlXCJdO1xyXG4gICAgICAgICQoXCIjdGFic1wiKS50YWJzKHtcclxuICAgICAgICAgICAgYWN0aXZhdGU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIGlmIChsb2NhbFN0b3JhZ2UgJiYgbG9jYWxTdG9yYWdlLnNldEl0ZW0pXHJcbiAgICAgICAgICAgICAgICAgICAgbG9jYWxTdG9yYWdlLnNldEl0ZW0oXCJzZWxlY3RlZEFzc2V0VGFiXCIsICQoXCIjdGFic1wiKS50YWJzKFwib3B0aW9uXCIsIFwiYWN0aXZlXCIpKTtcclxuICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgYWN0aXZlOiBsb2NhbFN0b3JhZ2UgJiYgbG9jYWxTdG9yYWdlLmdldEl0ZW0gPyBsb2NhbFN0b3JhZ2UuZ2V0SXRlbShcInNlbGVjdGVkQXNzZXRUYWJcIikgOiBudWxsXHJcbiAgICAgICAgfSkuc2hvdygpOyBcclxuICAgIH0pO1xyXG59Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9