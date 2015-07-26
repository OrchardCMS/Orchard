/// <reference path="Typings/jquery.d.ts" />

module Orchard.Azure.MediaServices.Admin.Common {
    $(() => {
        $("form").on("click", "button[data-prompt], a[data-prompt]", function(e) {
            var prompt = $(this).data("prompt");

            if (!confirm(prompt))
                e.preventDefault();
        });
    });
} 