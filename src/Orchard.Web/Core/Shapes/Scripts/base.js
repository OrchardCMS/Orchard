(function ($) {
    // Some simple settings storage/retrieval
    $.extend({
        orchard: {
            __cookieName: "Orchrd", // Orchard, on a diet
            __cookieExpiration: 180, // roughly 6 months
            ___doCookiePathHack: null,
            __doCookiePathHack: function (path) {
                if ($.orchard.___doCookiePathHack != null) {
                    return $.orchard.___doCookiePathHack;
                }
                var testCookie = "__orchard__";
                $.cookie(testCookie, "1", { path: path });
                $.orchard.___doCookiePathHack = !$.cookie(testCookie);
                $.cookie(testCookie, null);
                return $.orchard.___doCookiePathHack;
            },
            cookie: function (scope, value, options) { // a light-weight wrapper around $.cookie for an Orchard.* cookie name
                // because some browsers are weird about the path
                if (options && options.path && $.orchard.__doCookiePathHack(options.path)) {
                    options.path = options.path.substring(0, options.path.lastIndexOf("/"));
                }
                return $.cookie($.orchard.__cookieName + (scope ? scope.toLowerCase() : ""), value, options);
            },
            cookiesInTheOrchard: function () {
                return $.orchard.cookiesLike($.orchard.__cookieName);
            },
            cookiesLike: function (name) {
                var jar = [];
                // taken from the $.cookie plugin to get all of the cookies that begin with the name
                if (document.cookie && document.cookie != '') {
                    var cookies = document.cookie.split(';');
                    for (var i = 0; i < cookies.length; i++) {
                        var cookie = jQuery.trim(cookies[i]);
                        // Does this cookie string begin with the name we want?
                        if (cookie.split("=")[0].substring(0, name.length) === (name)) {
                            jar.push(decodeURIComponent(cookie.substring(cookie.indexOf("=") + 1)));
                        }
                    }
                }
                return jar;
            },
            setting: function (name, value, options) { // cookie-stored settings (only, at the moment)
                if (value && value.path) {
                    options = value;
                    value = undefined;
                }
                var key = (name + ((options && !!options.key && ("-" + options.key)) || "")).replace(/\W+/g, "-");
                if (typeof value === "undefined") { // try to get the setting value from the default "root" cookie
                    var cookies = $.orchard.cookiesInTheOrchard();
                    for (var i = 0; i < cookies.length; i++) {
                        var data = $.parseJSON(cookies[i]);
                        var value = data && data[key];
                        if (typeof value !== "undefined") {
                            return value;
                        }
                    }
                    return undefined;
                }
                else { // store the setting value - the setting isn't removable by the way, setting to "" might be enough for most cases
                    var scope = (options && options.path && options.path.replace(/\W+/g, "-")) || ""; // this could become a problem with long paths as it's appended to the cookie name
                    var cookie = $.orchard.cookie(scope);
                    var newData = (cookie && $.parseJSON(cookie)) || {};
                    newData[key] = value;
                    var dataString = (function (obj) { //todo: pull out into a seperate function
                        if (!obj) { return ""; }
                        var k, str = "{";
                        for (k in obj) { // only really simple stringification
                            str = str + "\"" + k + "\":\"" + obj[k] + "\",";
                        }
                        if (str.length > 1) {
                            str = str.substring(0, str.length - 1);
                        }
                        return str + "}";
                    })(newData);
                    $.orchard.cookie(scope, dataString, { expires: $.orchard.__cookieExpiration, path: (options && options.path) || "/" }); // todo: default path should be app path
                }
            }
        }
    });
    // Some input (auto)focus and input-controlled toggle
    $.fn.extend({
        helpfullyFocus: function () {
            var _this = $(this);
            var firstError = _this.find(".input-validation-error").first();
            // try to focus the first error on the page
            if (firstError.size() === 1) {
                firstError.focus();
                return _this;
            }
            // or, give it up to the browser to autofocus
            if ('autofocus' in document.createElement('input')) {
                return _this;
            }
            // otherwise, make the autofocus attribute work
            var autofocus = _this.find(":input[autofocus]").first();
            autofocus.focus();

            return _this;
        },
        helpfullyPlacehold: function () {
            var _this = $(this);

            // give it up to the browser to handle placeholder text
            if ('placeholder' in document.createElement('input')) {
                return _this;
            }
            // otherwise, make the placeholder attribute work
            $(":input[placeholder]")
                .each(function () {
                    var _this = $(this);
                    if (_this.val() === "") {
                        _this.val(_this.attr("placeholder")).addClass("placeholderd");
                    }
                })
                .on("focus", function () {
                    var _this = $(this);
                    if (_this.val() === _this.attr("placeholder")) {
                        _this.val("").removeClass("placeholderd");
                    }
                })
                .on("blur", function () {
                    var _this = $(this);
                    if (_this.val() === "") {
                        _this.val(_this.attr("placeholder")).addClass("placeholderd");
                    }
                });

            //make sure the placeholder value is not taken as the input value when submitting forms
            $("form").on("submit", function () {
                $(":input[placeholder].placeholderd").val("");
            });

            return _this;
        },
        toggleWhatYouControl: function () {
            var _this = $(this);
            var _controllees = $("[data-controllerid=" + _this.attr("id") + "]");

            _controllees.each(function () {
                var _controllee = $(this);

                var hiddenByDefault = _controllee.attr("data-defaultstate") == "hidden";
                var checkedOrSelected = _this.is(":checked") || _this.is(":selected");

                if (checkedOrSelected) {
                    if (!hiddenByDefault) {
                        _controllee.hide().show(); // <- unhook this when the following comment applies
                        // _controllees.slideUp(200); // <- hook this back up when chrome behaves, or when I care less...or when chrome behaves
                    } else {
                        _controllee.hide();
                    }
                } else {
                    if (!hiddenByDefault) {
                        _controllee.hide();
                    } else {
                        _controllee.show();
                    }
                }
            });

            _controllees.find("input").first().focus(); // <- aaaand a slideDown there...eventually

            return _this;
        }
    });
    // collapsable areas - anything with a data-controllerid attribute has its visibility controlled by the id-ed radio/checkbox
    $(function () {
        $("[data-controllerid]").each(function () {
            var controller = $("#" + $(this).attr("data-controllerid"));
            if (controller.data("isControlling")) {
                return;
            }
            controller.data("isControlling", 1);
            if (controller.is(":checkbox")) {
                controller.click($(this).toggleWhatYouControl).each($(this).toggleWhatYouControl);
            } else if (controller.is(":radio")) {
                $("[name='" + controller.attr("name") + "']").click(function () { $("[name='" + $(this).attr("name") + "']").each($(this).toggleWhatYouControl); }).each($(this).toggleWhatYouControl);
            }
            else if (controller.is("option")) {
                controller.parent().change(function() {
                    controller.toggleWhatYouControl();
                });
                controller.each($(this).toggleWhatYouControl);
            }
        });
    });
    // inline form link buttons (form.inline.link button) swapped out for a link that submits said form
    $(function () {
        $("form.inline.link").each(function () {
            var _this = $(this);
            var link = $("<a class='wasFormInlineLink' href='.'/>");
            var button = _this.children("button").first();
            link.text(button.text())
            .addClass(button.attr("class"))
            .click(function () { _this.submit(); return false; })
            .unload(function () { _this = 0; });
            _this.replaceWith(link);
            _this.css({ "position": "absolute", "left": "-9999em" });
            $("body").append(_this);
        });
    });
    // some default value add behavior
    $(function () {
        $("body").helpfullyFocus() // (do) a little better autofocus
            .helpfullyPlacehold(); // pick up on placeholders

    });
    // UnsafeUrl links -> form POST
    //todo: need some real microdata support eventually (incl. revisiting usage of data-* attributes)
    $(function () {
        var magicToken = $("input[name=__RequestVerificationToken]").first();
        if (!magicToken) { return; } // no sense in continuing if form POSTS will fail
        $("body").on("click", "a[itemprop~=UnsafeUrl], a[data-unsafe-url]", function() {
            var _this = $(this);
            var hrefParts = _this.attr("href").split("?");
            var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
            form.append(magicToken.clone());
            if (hrefParts.length > 1) {
                var queryParts = hrefParts[1].split("&");
                for (var i = 0; i < queryParts.length; i++) {
                    var queryPartKVP = queryParts[i].split("=");
                    //trusting hrefs in the page here
                    form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
                }
            }
            form.css({ "position": "absolute", "left": "-9999em" });
            $("body").append(form);

            var unsafeUrlPrompt = _this.data("unsafe-url");

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                if (!confirm(unsafeUrlPrompt)) {
                    return false;
                }
            }

            if (_this.filter("[itemprop~='RemoveUrl']").length == 1) {
            	// use a custom message if its set in data-message
            	var dataMessage = _this.data('message');
            	if (dataMessage === undefined) {
            		dataMessage = confirmRemoveMessage;
            	}

                if (!confirm(dataMessage)) {
                    return false;
                }
            }

            form.submit();
            return false;

        });
    });
})(jQuery);

///////////////////////////////////////////////////////////////
// --- some plugins leaned on by core script components      //
///////////////////////////////////////////////////////////////
/**
 * Cookie plugin
 *
 * Copyright (c) 2006 Klaus Hartl (stilbuero.de)
 * Dual licensed under the MIT and GPL licenses:
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.gnu.org/licenses/gpl.html
 *
 */

/**
 * Create a cookie with the given name and value and other optional parameters.
 *
 * @example $.cookie('the_cookie', 'the_value');
 * @desc Set the value of a cookie.
 * @example $.cookie('the_cookie', 'the_value', { expires: 7, path: '/', domain: 'jquery.com', secure: true });
 * @desc Create a cookie with all available options.
 * @example $.cookie('the_cookie', 'the_value');
 * @desc Create a session cookie.
 * @example $.cookie('the_cookie', null);
 * @desc Delete a cookie by passing null as value. Keep in mind that you have to use the same path and domain
 *       used when the cookie was set.
 *
 * @param String name The name of the cookie.
 * @param String value The value of the cookie.
 * @param Object options An object literal containing key/value pairs to provide optional cookie attributes.
 * @option Number|Date expires Either an integer specifying the expiration date from now on in days or a Date object.
 *                             If a negative value is specified (e.g. a date in the past), the cookie will be deleted.
 *                             If set to null or omitted, the cookie will be a session cookie and will not be retained
 *                             when the the browser exits.
 * @option String path The value of the path atribute of the cookie (default: path of page that created the cookie).
 * @option String domain The value of the domain attribute of the cookie (default: domain of page that created the cookie).
 * @option Boolean secure If true, the secure attribute of the cookie will be set and the cookie transmission will
 *                        require a secure protocol (like HTTPS).
 * @type undefined
 *
 * @name $.cookie
 * @cat Plugins/Cookie
 * @author Klaus Hartl/klaus.hartl@stilbuero.de
 */

/**
 * Get the value of a cookie with the given name.
 *
 * @example $.cookie('the_cookie');
 * @desc Get the value of a cookie.
 *
 * @param String name The name of the cookie.
 * @return The value of the cookie.
 * @type String
 *
 * @name $.cookie
 * @cat Plugins/Cookie
 * @author Klaus Hartl/klaus.hartl@stilbuero.de
 */
jQuery.cookie = function(name, value, options) {
    if (typeof value != 'undefined') { // name and value given, set cookie
        options = options || {};
        if (value === null) {
            value = '';
            options.expires = -1;
        }
        var expires = '';
        if (options.expires && (typeof options.expires == 'number' || options.expires.toUTCString)) {
            var date;
            if (typeof options.expires == 'number') {
                date = new Date();
                date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000));
            } else {
                date = options.expires;
            }
            expires = '; expires=' + date.toUTCString(); // use expires attribute, max-age is not supported by IE
        }
        // CAUTION: Needed to parenthesize options.path and options.domain
        // in the following expressions, otherwise they evaluate to undefined
        // in the packed version for some reason...
        var path = options.path ? '; path=' + (options.path) : '';
        var domain = options.domain ? '; domain=' + (options.domain) : '';
        var secure = options.secure ? '; secure' : '';
        document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
    } else { // only name given, get cookie
        var cookieValue = null;
        if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]);
                // Does this cookie string begin with the name we want?
                if (cookie.substring(0, name.length + 1) == (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
};