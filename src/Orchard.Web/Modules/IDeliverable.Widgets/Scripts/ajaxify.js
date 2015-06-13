/// <reference path="typings/jquery/jquery.d.ts" />
var IDeliverable;
(function (IDeliverable) {
    var AjaxWidget;
    (function (AjaxWidget) {
        $(function () {
            $(".widget-ajax-placeholder").each(function () {
                var placeholder = $(this);
                var loader = placeholder.find(".widget-ajax-loader");
                var errorLabel = placeholder.find(".widget-ajax-error");
                var ajaxUrl = placeholder.data("widget-ajax-url");
                var parent = placeholder.parent();
                if (ajaxUrl) {
                    var update = function (url, target) {
                        errorLabel.hide();
                        loader.show();
                        $.get(url, function (html) {
                            var newContent = $(html);
                            target.replaceWith(newContent);
                            // Process local urls, such as pager urls.
                            newContent.on("click", "a[href^='" + ajaxUrl + "']", function (e) {
                                update($(this).attr("href"), newContent);
                                e.preventDefault();
                            });
                        }).fail(function () {
                            errorLabel.show();
                            loader.hide();
                        });
                    };
                    update(ajaxUrl, parent);
                }
            });
        });
    })(AjaxWidget = IDeliverable.AjaxWidget || (IDeliverable.AjaxWidget = {}));
})(IDeliverable || (IDeliverable = {}));
//# sourceMappingURL=ajaxify.js.map