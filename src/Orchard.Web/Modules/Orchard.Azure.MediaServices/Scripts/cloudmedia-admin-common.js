/// <reference path="typings/jquery.d.ts" />
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (Admin) {
                (function (Common) {
                    $(function () {
                        $("form").on("click", "button[data-prompt], a[data-prompt]", function (e) {
                            var prompt = $(this).data("prompt");

                            if (!confirm(prompt))
                                e.preventDefault();
                        });
                    });
                })(Admin.Common || (Admin.Common = {}));
                var Common = Admin.Common;
            })(MediaServices.Admin || (MediaServices.Admin = {}));
            var Admin = MediaServices.Admin;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-admin-common.js.map
