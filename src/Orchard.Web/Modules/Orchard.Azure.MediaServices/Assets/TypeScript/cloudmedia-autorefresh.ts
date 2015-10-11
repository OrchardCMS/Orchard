/// <reference path="Typings/jquery.d.ts" />

module Orchard.Azure.MediaServices.AutoRefresh {
    // Periodically refresh elements.
    $(() => {
        $("[data-refresh-url]").each(function () {
            var self = $(this);
            var update = () => {
                var container = self;
                var url = container.data("refresh-url");

                $.ajax({
                    url: url,
                    cache: false
                }).then(html => {
                    container.html(html);
                    setTimeout(update, 5000);
                });
            };

            setTimeout(update, 5000);
        });
    });
}