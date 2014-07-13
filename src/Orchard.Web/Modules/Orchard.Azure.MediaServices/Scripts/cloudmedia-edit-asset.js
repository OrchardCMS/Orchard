/// <reference path="typings/jquery.d.ts" />
/// <reference path="typings/jqueryui.d.ts" />
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
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
            })(MediaServices.AssetEdit || (MediaServices.AssetEdit = {}));
            var AssetEdit = MediaServices.AssetEdit;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-edit-asset.js.map
