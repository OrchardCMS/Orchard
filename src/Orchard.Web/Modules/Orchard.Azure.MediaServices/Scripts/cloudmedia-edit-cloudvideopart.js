/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var CloudVideoEdit;
            (function (CloudVideoEdit) {
                function hasCorsSupport() {
                    return 'withCredentials' in new XMLHttpRequest();
                }
                $(function () {
                    var corsSupported = hasCorsSupport();
                    if (corsSupported) {
                        CloudVideoEdit.initializeUploadDirect();
                    }
                    else {
                        CloudVideoEdit.initializeUploadProxied();
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
            })(CloudVideoEdit = MediaServices.CloudVideoEdit || (MediaServices.CloudVideoEdit = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
