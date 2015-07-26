/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />

module Orchard.Azure.MediaServices.AssetEdit {
    $(function() {
        var localStorage = window["localStorage"];
        $("#tabs").tabs({
            activate: function () {
                if (localStorage && localStorage.setItem)
                    localStorage.setItem("selectedAssetTab", $("#tabs").tabs("option", "active"));
            },
            active: localStorage && localStorage.getItem ? localStorage.getItem("selectedAssetTab") : null
        }).show(); 
    });
}