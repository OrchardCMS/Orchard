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
                var Video;
                (function (Video) {
                    $(function () {
                        var treeView = $("#asset-files-treeview");
                        treeView.jstree({
                            "core": {
                                "animation": 0,
                                "check_callback": true,
                                "themes": { "stripes": true }
                            },
                            "plugins": ["state", "wholerow"]
                        });
                        $(".expand-all").on("click", function (e) {
                            treeView.jstree('open_all');
                        });
                        $(".collapse-all").on("click", function (e) {
                            treeView.jstree('close_all');
                        });
                        // TODO: Make links work (Private/Public URLS).
                        //treeView.on("select_node.jstree", function (e, data) {
                        //    var url = data.node.a_attr.href;
                        //    if (url != "#") {
                        //        window.location.href = url;
                        //    }
                        //});
                    });
                })(Video = AssetEdit.Video || (AssetEdit.Video = {}));
            })(AssetEdit = MediaServices.AssetEdit || (MediaServices.AssetEdit = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb3VkbWVkaWEtZWRpdC1hc3NldC12aWRlby50cyJdLCJuYW1lcyI6WyJPcmNoYXJkIiwiT3JjaGFyZC5BenVyZSIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcyIsIk9yY2hhcmQuQXp1cmUuTWVkaWFTZXJ2aWNlcy5Bc3NldEVkaXQiLCJPcmNoYXJkLkF6dXJlLk1lZGlhU2VydmljZXMuQXNzZXRFZGl0LlZpZGVvIl0sIm1hcHBpbmdzIjoiQUFBQSw0Q0FBNEM7QUFDNUMsOENBQThDO0FBRTlDLElBQU8sT0FBTyxDQTZCYjtBQTdCRCxXQUFPLE9BQU87SUFBQ0EsSUFBQUEsS0FBS0EsQ0E2Qm5CQTtJQTdCY0EsV0FBQUEsS0FBS0E7UUFBQ0MsSUFBQUEsYUFBYUEsQ0E2QmpDQTtRQTdCb0JBLFdBQUFBLGFBQWFBO1lBQUNDLElBQUFBLFNBQVNBLENBNkIzQ0E7WUE3QmtDQSxXQUFBQSxTQUFTQTtnQkFBQ0MsSUFBQUEsS0FBS0EsQ0E2QmpEQTtnQkE3QjRDQSxXQUFBQSxLQUFLQSxFQUFDQSxDQUFDQTtvQkFDaERDLENBQUNBLENBQUNBO3dCQUNFLElBQUksUUFBUSxHQUFRLENBQUMsQ0FBQyx1QkFBdUIsQ0FBQyxDQUFDO3dCQUUvQyxRQUFRLENBQUMsTUFBTSxDQUFDOzRCQUNaLE1BQU0sRUFBRTtnQ0FDSixXQUFXLEVBQUUsQ0FBQztnQ0FDZCxnQkFBZ0IsRUFBRSxJQUFJO2dDQUN0QixRQUFRLEVBQUUsRUFBRSxTQUFTLEVBQUUsSUFBSSxFQUFFOzZCQUNoQzs0QkFDRCxTQUFTLEVBQUUsQ0FBQyxPQUFPLEVBQUUsVUFBVSxDQUFDO3lCQUNuQyxDQUFDLENBQUM7d0JBRUgsQ0FBQyxDQUFDLGFBQWEsQ0FBQyxDQUFDLEVBQUUsQ0FBQyxPQUFPLEVBQUUsVUFBUyxDQUFDOzRCQUNuQyxRQUFRLENBQUMsTUFBTSxDQUFDLFVBQVUsQ0FBQyxDQUFDO3dCQUNoQyxDQUFDLENBQUMsQ0FBQzt3QkFFSCxDQUFDLENBQUMsZUFBZSxDQUFDLENBQUMsRUFBRSxDQUFDLE9BQU8sRUFBRSxVQUFVLENBQUM7NEJBQ3RDLFFBQVEsQ0FBQyxNQUFNLENBQUMsV0FBVyxDQUFDLENBQUM7d0JBQ2pDLENBQUMsQ0FBQyxDQUFDO3dCQUVILCtDQUErQzt3QkFDL0Msd0RBQXdEO3dCQUN4RCxzQ0FBc0M7d0JBQ3RDLHVCQUF1Qjt3QkFDdkIscUNBQXFDO3dCQUNyQyxPQUFPO3dCQUNQLEtBQUs7b0JBQ1QsQ0FBQyxDQUFDQSxDQUFDQTtnQkFDUEEsQ0FBQ0EsRUE3QjRDRCxLQUFLQSxHQUFMQSxlQUFLQSxLQUFMQSxlQUFLQSxRQTZCakRBO1lBQURBLENBQUNBLEVBN0JrQ0QsU0FBU0EsR0FBVEEsdUJBQVNBLEtBQVRBLHVCQUFTQSxRQTZCM0NBO1FBQURBLENBQUNBLEVBN0JvQkQsYUFBYUEsR0FBYkEsbUJBQWFBLEtBQWJBLG1CQUFhQSxRQTZCakNBO0lBQURBLENBQUNBLEVBN0JjRCxLQUFLQSxHQUFMQSxhQUFLQSxLQUFMQSxhQUFLQSxRQTZCbkJBO0FBQURBLENBQUNBLEVBN0JNLE9BQU8sS0FBUCxPQUFPLFFBNkJiIiwiZmlsZSI6ImNsb3VkbWVkaWEtZWRpdC1hc3NldC12aWRlby5qcyIsInNvdXJjZXNDb250ZW50IjpbIi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeS5kLnRzXCIgLz5cbi8vLyA8cmVmZXJlbmNlIHBhdGg9XCJUeXBpbmdzL2pxdWVyeXVpLmQudHNcIiAvPlxuXG5tb2R1bGUgT3JjaGFyZC5BenVyZS5NZWRpYVNlcnZpY2VzLkFzc2V0RWRpdC5WaWRlbyB7XG4gICAgJChmdW5jdGlvbiAoKSB7XG4gICAgICAgIHZhciB0cmVlVmlldzogYW55ID0gJChcIiNhc3NldC1maWxlcy10cmVldmlld1wiKTtcblxuICAgICAgICB0cmVlVmlldy5qc3RyZWUoe1xuICAgICAgICAgICAgXCJjb3JlXCI6IHtcbiAgICAgICAgICAgICAgICBcImFuaW1hdGlvblwiOiAwLFxuICAgICAgICAgICAgICAgIFwiY2hlY2tfY2FsbGJhY2tcIjogdHJ1ZSxcbiAgICAgICAgICAgICAgICBcInRoZW1lc1wiOiB7IFwic3RyaXBlc1wiOiB0cnVlIH0sXG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgXCJwbHVnaW5zXCI6IFtcInN0YXRlXCIsIFwid2hvbGVyb3dcIl1cbiAgICAgICAgfSk7XG5cbiAgICAgICAgJChcIi5leHBhbmQtYWxsXCIpLm9uKFwiY2xpY2tcIiwgZnVuY3Rpb24oZSkge1xuICAgICAgICAgICAgdHJlZVZpZXcuanN0cmVlKCdvcGVuX2FsbCcpO1xuICAgICAgICB9KTtcblxuICAgICAgICAkKFwiLmNvbGxhcHNlLWFsbFwiKS5vbihcImNsaWNrXCIsIGZ1bmN0aW9uIChlKSB7XG4gICAgICAgICAgICB0cmVlVmlldy5qc3RyZWUoJ2Nsb3NlX2FsbCcpO1xuICAgICAgICB9KTtcblxuICAgICAgICAvLyBUT0RPOiBNYWtlIGxpbmtzIHdvcmsgKFByaXZhdGUvUHVibGljIFVSTFMpLlxuICAgICAgICAvL3RyZWVWaWV3Lm9uKFwic2VsZWN0X25vZGUuanN0cmVlXCIsIGZ1bmN0aW9uIChlLCBkYXRhKSB7XG4gICAgICAgIC8vICAgIHZhciB1cmwgPSBkYXRhLm5vZGUuYV9hdHRyLmhyZWY7XG4gICAgICAgIC8vICAgIGlmICh1cmwgIT0gXCIjXCIpIHtcbiAgICAgICAgLy8gICAgICAgIHdpbmRvdy5sb2NhdGlvbi5ocmVmID0gdXJsO1xuICAgICAgICAvLyAgICB9XG4gICAgICAgIC8vfSk7XG4gICAgfSk7XG59Il0sInNvdXJjZVJvb3QiOiIvc291cmNlLyJ9