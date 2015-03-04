/// <reference path="typings/jquery.d.ts" />
var Orchard;
(function (Orchard) {
    var Azure;
    (function (Azure) {
        var MediaServices;
        (function (MediaServices) {
            var AutoRefresh;
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
            })(AutoRefresh = MediaServices.AutoRefresh || (MediaServices.AutoRefresh = {}));
        })(MediaServices = Azure.MediaServices || (Azure.MediaServices = {}));
    })(Azure = Orchard.Azure || (Orchard.Azure = {}));
})(Orchard || (Orchard = {}));
