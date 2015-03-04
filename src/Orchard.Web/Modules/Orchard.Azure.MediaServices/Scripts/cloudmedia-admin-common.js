/// <reference path="typings/jquery.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var Admin;
            (function (Admin) {
                var Common;
                (function (Common) {
                    $(function () {
                        $("form").on("click", "button[data-prompt], a[data-prompt]", function (e) {
                            var prompt = $(this).data("prompt");
                            if (!confirm(prompt))
                                e.preventDefault();
                        });
                    });
                })(Common = Admin.Common || (Admin.Common = {}));
            })(Admin = MediaServices.Admin || (MediaServices.Admin = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
