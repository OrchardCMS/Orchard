(function ($) {
    //todo: (heskew) make use of the autofocus attribute instead
    $.fn.extend({
        helpfullyFocus: function () {
            var _this = $(this);
            var firstError = _this.find(".input-validation-error").first();
            // try to focus the first error on the page
            if (firstError.size() === 1) {
                return firstError.focus();
            }
            // or, give it up to the browser to autofocus
            if ('autofocus' in document.createElement('input')) {
                return;
            }
            // otherwise, make the autofocus attribute work
            var autofocus = _this.find(":input[autofocus=autofocus]").first();
            return autofocus.focus();
        },
        toggleWhatYouControl: function () {
            var _this = $(this);
            var _controllees = $("[data-controllerid=" + _this.attr("id") + "]");
            var _controlleesAreHidden = _controllees.is(":hidden");
            if (_this.is(":checked") && _controlleesAreHidden) {
                _controllees.hide(); // <- unhook this when the following comment applies
                $(_controllees.show()[0]).find("input").focus(); // <- aaaand a slideDown there...eventually
            } else if (!(_this.is(":checked") && _controlleesAreHidden)) {
                //_controllees.slideUp(200); <- hook this back up when chrome behaves, or when I care less
                _controllees.hide()
            }
        }
    });
    // collapsable areas - anything with a data-controllerid attribute has its visibility controlled by the id-ed radio/checkbox
    (function () {
        $("[data-controllerid]").each(function () {
            var controller = $("#" + $(this).attr("data-controllerid"));
            if (controller.data("isControlling")) {
                return;
            }
            controller.data("isControlling", 1);
            if (!controller.is(":checked")) {
                $("[data-controllerid=" + controller.attr("id") + "]").hide();
            }
            if (controller.is(":checkbox")) {
                controller.click($(this).toggleWhatYouControl);
            } else if (controller.is(":radio")) {
                $("[name=" + controller.attr("name") + "]").click(function () { $("[name=" + $(this).attr("name") + "]").each($(this).toggleWhatYouControl); });
            }
        });
    })();
    // inline form link buttons (form.inline.link button) swapped out for a link that submits said form
    (function () {
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
    })();
    // a little better autofocus
    $(function () {
        $("body").helpfullyFocus();
    });
    // UnsafeUrl links -> form POST
    //todo: need some real microdata support eventually (incl. revisiting usage of data-* attributes)
    $(function () {
        var magicToken = $("input[name=__RequestVerificationToken]").first();
        if (!magicToken) { return; } // no sense in continuing if form POSTS will fail
        $("a[itemprop~=UnsafeUrl]").each(function () {
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
            _this.click(function () { form.submit(); return false; });
        });
    });
})(jQuery);