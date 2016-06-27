/// <reference path="Typings/jquery.d.ts" />
/// <reference path="Typings/jqueryui.d.ts" />

module Orchard.Azure.MediaServices.CloudVideoEdit {
    function hasCorsSupport() {
        return 'withCredentials' in new XMLHttpRequest();
    }

    $(function() {
        var corsSupported = hasCorsSupport();

        if (corsSupported) {
            initializeUploadDirect();
        } else {
            initializeUploadProxied();
        }

        var localStorage = window["localStorage"];
        var isCreating: boolean = $("#tabs").data("cloudvideo-iscreating");
        $("#tabs").tabs({
            activate: function () {
                if (localStorage && localStorage.setItem)
                    localStorage.setItem("selectedCloudVideoTab", $("#tabs").tabs("option", "active"));
            },
            active: !isCreating && localStorage && localStorage.getItem ? localStorage.getItem("selectedCloudVideoTab") : null
        }).show();
    });
}