(function ($) {
    // Hide the Layer, Zone, and Position fieldsets and set a value for the required fields.
    $(function() {
        var fieldsets = $("#element-properties .edit-widget fieldset").slice(0, 3);
        fieldsets.hide();

        $("input[name='WidgetPart.Position']").val("0");
    });
})(jQuery);