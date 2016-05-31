/*!
 * jQuery bootstrap 3 breakpoint check
 * Check the current visibility of bootstrap 3 breakpoints
 *
 * @example `$.isXs()` function alias for `$.isBreakpoint("xs")`
 * @example `$.isSm()` function alias for `$.isBreakpoint("sm")`
 * @example `$.isMd()` function alias for `$.isBreakpoint("md")`
 * @example `$.isLg()` function alias for `$.isBreakpoint("lg")`
 * @version 1.0.0
 * @copyright Jens A. (cakebake) and other contributors
 * @license Released under the MIT license
 */

;(function ($) {

    /**
     * Base function to check the current visibility of an bootstrap 3 breakpoint
     *
     * @param {string} breakPoint Something like xs|sm|md|lg
     * @returns {boolean}
     */
    $.isBreakpoint = function (breakPoint) {
        var element, erg;

        element = $("<div/>", {
            "class": "visible-" + breakPoint
        }).appendTo("body");

        erg = element.is(":visible");
        element.remove();

        return erg
    };

    $.extend($, {

        /**
         * Check the current visibility of bootstrap 3 "xs" breakpoint
         * Shorthand function alias for $.isBreakpoint("xs")
         *
         * @returns {boolean}
         */
        isXs: function () {
            return $.isBreakpoint("xs")
        },

        /**
         * Check the current visibility of bootstrap 3 "sm" breakpoint
         * Shorthand function alias for $.isBreakpoint("sm")
         *
         * @returns {boolean}
         */
        isSm: function () {
            return $.isBreakpoint("sm")
        },

        /**
         * Check the current visibility of bootstrap 3 "md" breakpoint
         * Shorthand function alias for $.isBreakpoint("md")
         *
         * @returns {boolean}
         */
        isMd: function () {
            return $.isBreakpoint("md")
        },

        /**
         * Check the current visibility of bootstrap 3 "lg" breakpoint
         * Shorthand function alias for $.isBreakpoint("lg")
         *
         * @returns {boolean}
         */
        isLg: function () {
            return $.isBreakpoint("lg")
        }

    });

})(jQuery);
