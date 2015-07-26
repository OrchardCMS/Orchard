/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />

module Orchard.Azure.MediaServices.AssetEdit.Video {
    $(function () {
        var treeView: any = $("#asset-files-treeview");

        treeView.jstree({
            "core": {
                "animation": 0,
                "check_callback": true,
                "themes": { "stripes": true },
            },
            "plugins": ["state", "wholerow"]
        });

        $(".expand-all").on("click", function(e) {
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
}