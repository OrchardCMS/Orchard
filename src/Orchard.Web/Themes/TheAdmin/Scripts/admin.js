(function ($) {
    $.fn.extend({
        expandoControl: function (getControllees, options) {
            if (typeof getControllees !== "function")
                return this;

            var _this = $(this);
            var __cookieName = "Exp";
            var settings = $.extend({
                path: "/",
                key: _this.selector,
                collapse: false,
                remember: true
            }, options);
            _this.each(function (index, element) {
                var controller = $(element);
                var glyph = $("<span class=\"expandoGlyph\"></span>");

                glyph.data("controllees", getControllees(controller));
                if ((settings.remember && "closed" === $.orchard.setting(__cookieName, { key: settings.key + "-" + controller.text(), path: settings.path }))
                    || settings.collapse) {
                    glyph.addClass("closed").data("controllees").hide();
                }
                else if (settings.collapse) {

                }

                glyph.click(function () {
                    var __this = $(this);

                    if (settings.remember && !settings.collapse) { // remembering closed state as true because that's not the default - doesn't make sense to remember if the controllees are always to be collapsed by default
                        // need to allow specified keys since these selectors could get *really* long
                        $.orchard.setting(__cookieName, !__this.hasClass("closed") ? "closed" : "open", { key: settings.key + "-" + controller.text(), path: settings.path });
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