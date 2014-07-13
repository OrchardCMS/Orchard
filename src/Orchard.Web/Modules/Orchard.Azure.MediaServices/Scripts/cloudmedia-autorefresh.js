/// <reference path="typings/jquery.d.ts" />
var Orchard;
(function (Orchard) {
    (function (Azure) {
        (function (MediaServices) {
            (function (AutoRefresh) {
                // Periodically refresh elements.
                $(function () {
                    $("[data-refresh-url]").each(function () {
                        var self = $(this);
                        var update = function () {
                            var container = self;
                            var url = container.data("refresh-url");

                            $.ajax({
                                url: url,
                                cache: false
                            }).then(function (html) {
                                container.html(html);
                                setTimeout(update, 5000);
                            });
                        };

                        setTimeout(update, 5000);
                    });
                });
            })(MediaServices.AutoRefresh || (MediaServices.AutoRefresh = {}));
            var AutoRefresh = MediaServices.AutoRefresh;
        })(Azure.MediaServices || (Azure.MediaServices = {}));
        var MediaServices = Azure.MediaServices;
    })(Orchard.Azure || (Orchard.Azure = {}));
    var Azure = Orchard.Azure;
})(Orchard || (Orchard = {}));
//# sourceMappingURL=cloudmedia-autorefresh.js.map
