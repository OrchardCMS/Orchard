/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (CloudVideoEdit) {
                function hasCorsSupport() {
                    return 'withCredentials' in new XMLHttpRequest();
                }

                $(function () {
                    var corsSupported = hasCorsSupport();

                    if (corsSupported) {
                        Orchard.Azure.MediaServices.CloudVideoEdit.initializeUploadDirect();
                    } else {
                        Orchard.Azure.MediaServices.CloudVideoEdit.initializeUploadProxied();
                    }

                    var localStorage = window["localStorage"];
                    var isCreating = $("#tabs").data("cloudvideo-iscreating");
                    $("#tabs").tabs({
                        activate: function () {
                            if (localStorage && localStorage.setItem)
                                localStorage.setItem("selectedCloudVideoTab", $("#tabs").tabs("option", "active"));
                        },
                        active: !isCreating && localStorage && localStorage.getItem ? localStorage.getItem("selectedCloudVideoTab") : null
                    }).show();
                });
            })(MediaServices.CloudVideoEdit || (MediaServices.CloudVideoEdit = {}));
            var CloudVideoEdit = MediaServices.CloudVideoEdit;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-edit-cloudvideopart.js.map
