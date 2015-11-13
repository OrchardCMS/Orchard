(function ($) {
    var placeable = $("input[name=\"ContentPartLayoutSettings.Placeable\"]");

    $(placeable).on("change", function (e) {
        syncEnableEditorInput();
    });

    $(function () {
        syncEnableEditorInput();
    });

    var syncEnableEditorInput = function () {
        var enableEditorDialog = $("input[name=\"ContentPartLayoutSettings.EnableEditorDialog\"]");
        var isPlaceable = placeable.is(":checked");

        enableEditorDialog.prop("disabled", !isPlaceable);
    };

})(jQuery);