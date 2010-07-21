(function ($) {
    $.fn.extend({
        expandoControl: function (getControllees, options) {
            if (typeof getControllees !== "function")
                return this;

            var _this = $(this);
            var settings = $.extend({
                path: "/",
                collapse: false,
                remember: true
            }, options);
            _this.each(function (index, element) {
                var controller = $(element);
                var glyph = $("<span class=\"expandoGlyph\"></span>");

                glyph.data("controllees", getControllees(controller));
                if (settings.remember) {
                    var state = $.orchard.setting("expando", { key: _this.selector + "-" + controller.text(), path: settings.path });
                    if (state === "closed") {
                        glyph.addClass("closed").data("controllees").hide();
                    }
                }
                glyph.click(function () {
                    var __this = $(this);

                    if (settings.remember) { // remembering closed state as true because that's not the default
                        // need to allow specified keys since these selectors could get *really* long
                        $.orchard.setting("expando", !__this.hasClass("closed") ? "closed" : "open", { key: _this.selector + "-" + controller.text(), path: settings.path });
                    }

                    if (__this.hasClass("closed") || __this.hasClass("closing")) {
                        __this.addClass("opening")
                            .data("controllees").slideDown(300, function () { __this.removeClass("opening").removeClass("closed").addClass("open"); });
                    }
                    else {
                        __this.addClass("closing")
                            .data("controllees").slideUp(300, function () { __this.removeClass("closing").removeClass("open").addClass("closed"); });
                    }

                    return false;
                });

                controller.before(glyph);
            });

            return this;
        }
    });
})(jQuery);