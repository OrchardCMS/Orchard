/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/jqueryui.d.ts" />
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
