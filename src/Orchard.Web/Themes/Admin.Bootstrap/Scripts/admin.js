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
                var glyph = $("<span class=\"expando-glyph-container\"><span class=\"expando-glyph\"></span>&#8203;</span>");

                glyph.data("controllees", getControllees(controller));

                if (glyph.data("controllees").length === 0 || glyph.data("controllees").height() < 1) {
                    return;
                }

                if ((settings.remember && "closed" === $.orchard.setting(__cookieName, { key: settings.key + "-" + controller.text(), path: settings.path }))
                    || settings.collapse
                    || (controller.closest("li").hasClass("collapsed") && !(settings.remember && "open" === $.orchard.setting(__cookieName, { key: settings.key + "-" + controller.text(), path: settings.path })))) {
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
                        __this.data("controllees").slideDown(300, function () { __this.removeClass("opening").removeClass("closed").addClass("open"); });
                        __this.addClass("opening");
                    }
                    else {
                        __this.data("controllees").slideUp(300, function () { __this.removeClass("closing").removeClass("open").addClass("closed"); });
                        __this.addClass("closing");
                    }

                    return false;
                });

                controller.prepend(glyph);
            });

            return this;
        }
    });

    $(".bulk-actions-auto select").change(function () {
        $(this).closest("form").find(".apply-bulk-actions-auto:first").click();
    });

    $("body").on("click", "[itemprop~='RemoveUrl']", function () {
        // don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled in base.js
        if ($(this).filter("[itemprop~='UnsafeUrl']").length == 1) {
            return false;
        }

        return confirm(confirmRemoveMessage);
    });
    
    $(".check-all").change(function () {
        $(this).parents("table.items").find(":checkbox:not(:disabled)").prop('checked', $(this).prop("checked"));
    }); 
})(jQuery);


/*
 console-shim 1.0.2
 https://github.com/kayahr/console-shim
 Copyright (C) 2011 Klaus Reimer <k@ailis.de>
 Licensed under the MIT license
 (See http://www.opensource.org/licenses/mit-license)
*/
function f() { return function () { } }
(function () {
    function c(a, l, b) { var c = Array.prototype.slice.call(arguments, 2); return function () { var b = c.concat(Array.prototype.slice.call(arguments, 0)); a.apply(l, b) } } window.console || (window.console = {}); var a = window.console; if (!a.log) if (window.log4javascript) { var b = log4javascript.getDefaultLogger(); a.log = c(b.info, b); a.debug = c(b.debug, b); a.info = c(b.info, b); a.warn = c(b.warn, b); a.error = c(b.error, b) } else a.log = f(); a.debug || (a.debug = a.log); a.info || (a.info = a.log); a.warn || (a.warn = a.log); a.error || (a.error = a.log);
    if (null != window.__consoleShimTest__ || eval("/*@cc_on @_jscript_version \x3c\x3d 9@*/")) b = function (d) { var b, e, c; d = Array.prototype.slice.call(arguments, 0); c = d.shift(); e = d.length; if (1 < e && !1 !== window.__consoleShimTest__) { "string" != typeof d[0] && (d.unshift("%o"), e += 1); for (b = (b = d[0].match(/%[a-z]/g)) ? b.length + 1 : 1; b < e; b += 1) d[0] += " %o" } Function.apply.call(c, a, d) }, a.log = c(b, window, a.log), a.debug = c(b, window, a.debug), a.info = c(b, window, a.info), a.warn = c(b, window, a.warn), a.error = c(b, window, a.error); a.assert || (a.assert =
    function () { var d = Array.prototype.slice.call(arguments, 0); d.shift() || (d[0] = "Assertion failed: " + d[0], a.error.apply(a, d)) }); a.dir || (a.dir = a.log); a.dirxml || (a.dirxml = a.log); a.exception || (a.exception = a.error); if (!a.time || !a.timeEnd) { var g = {}; a.time = function (a) { g[a] = (new Date).getTime() }; a.timeEnd = function (b) { var c = g[b]; c && (a.log(b + ": " + ((new Date).getTime() - c) + "ms"), delete g[b]) } } a.table || (a.table = function (b, c) {
        var e, g, j, h, k; if (b && b instanceof Array && b.length) {
            if (!c || !(c instanceof Array)) for (e in c =
            [], b[0]) b[0].hasOwnProperty(e) && c.push(e); e = 0; for (g = b.length; e < g; e += 1) { j = []; h = 0; for (k = c.length; h < k; h += 1) j.push(b[e][c[h]]); Function.apply.call(a.log, a, j) }
        }
    }); a.clear || (a.clear = f()); a.trace || (a.trace = f()); a.group || (a.group = f()); a.groupCollapsed || (a.groupCollapsed = f()); a.groupEnd || (a.groupEnd = f()); a.timeStamp || (a.timeStamp = f()); a.profile || (a.profile = f()); a.profileEnd || (a.profileEnd = f()); a.count || (a.count = f())
})();