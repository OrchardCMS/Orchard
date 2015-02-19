angular
    .module("LayoutEditor")
    .directive("orcLayoutPopup", function () {
        return {
            restrict: "A",
            link: function (scope, element, attrs) {
                var popup = $(element);
                var trigger = popup.closest(".layout-popup-trigger");
                var parentElement = popup.closest(".layout-element");
                trigger.click(function () {
                    popup.toggle();
                    if (popup.is(":visible")) {
                        popup.position({
                            my: attrs.orcLayoutPopupMy || "left top",
                            at: attrs.orcLayoutPopupAt || "left bottom+4px",
                            of: trigger
                        });
                    }
                });
                popup.click(function (e) {
                    e.stopPropagation();
                });
                parentElement.click(function (e) {
                    popup.hide();
                });
            }
        };
    });