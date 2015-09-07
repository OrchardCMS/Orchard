(function ($) {
    // Hide the Layer, Zone, and Position fieldsets.
    $(function() {
        var fieldsets = $("#element-properties .edit-widget fieldset").slice(0, 3);
        fieldsets.hide();
    });
})(jQuery);