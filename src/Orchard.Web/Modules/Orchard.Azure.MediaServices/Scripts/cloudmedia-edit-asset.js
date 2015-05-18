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
