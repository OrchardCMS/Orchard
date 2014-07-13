/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (AssetEdit) {
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
                })(AssetEdit.Video || (AssetEdit.Video = {}));
                var Video = AssetEdit.Video;
            })(MediaServices.AssetEdit || (MediaServices.AssetEdit = {}));
            var AssetEdit = MediaServices.AssetEdit;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-edit-asset-video.js.map
