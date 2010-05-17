//todo: (heskew) make use of the autofocus attribute instead
jQuery.fn.extend({
    helpfullyFocus: function() {
        var _this = jQuery(this);
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
    toggleWhatYouControl: function() {
        var _this = jQuery(this);
        var _controllees = jQuery("[data-controllerid=" + _this.attr("id") + "]");
        var _controlleesAreHidden = _controllees.is(":hidden");
        if (_this.is(":checked") && _controlleesAreHidden) {
            _controllees.hide(); // <- unhook this when the following comment applies
            jQuery(_controllees.show()[0]).find("input").focus(); // <- aaaand a slideDown there...eventually
        } else if (!(_this.is(":checked") && _controlleesAreHidden)) {
            //_controllees.slideUp(200); <- hook this back up when chrome behaves, or when I care less
            _controllees.hide()
        }
    }
});
// collapsable areas - anything with a data-controllerid attribute has its visibility controlled by the id-ed radio/checkbox
(function() {
    jQuery("[data-controllerid]").each(function() {
        var controller = jQuery("#" + jQuery(this).attr("data-controllerid"));
        if (controller.data("isControlling")) {
            return;
        }
        controller.data("isControlling", 1);
        if (!controller.is(":checked")) {
            jQuery("[data-controllerid=" + controller.attr("id") + "]").hide();
        }
        if (controller.is(":checkbox")) {
            controller.click(jQuery(this).toggleWhatYouControl);
        } else if (controller.is(":radio")) {
            jQuery("[name=" + controller.attr("name") + "]").click(function() { jQuery("[name=" + jQuery(this).attr("name") + "]").each(jQuery(this).toggleWhatYouControl); });
        }
    });
})();
// inline form link buttons (form.inline.link button) swapped out for a link that submits said form
(function() {
    jQuery("form.inline.link").each(function() {
        var _this = jQuery(this);
        var link = jQuery("<a class='wasFormInlineLink' href='.'/>");
        var button = _this.children("button").first();
        link.text(button.text())
            .addClass(button.attr("class"))
            .click(function() { _this.submit(); return false; })
            .unload(function() { _this = 0; });
        _this.replaceWith(link);
        _this.css({ "position": "absolute", "left": "-9999em" });
        jQuery("body").append(_this);
    });
})();
// a little better autofocus
jQuery(function() {
    jQuery("body").helpfullyFocus();
});