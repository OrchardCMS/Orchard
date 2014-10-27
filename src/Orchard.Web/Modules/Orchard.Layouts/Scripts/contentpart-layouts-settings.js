(function ($) {
    var placable = $("input[name=\"ContentPartLayoutSettings.Placable\"]");

    $(placable).on("change", function (e) {
        syncEnableEditorInput();
    });

    $(function () {
        syncEnableEditorInput();
    });

    var syncEnableEditorInput = function () {
        var enableEditorDialog = $("input[name=\"ContentPartLayoutSettings.EnableEditorDialog\"]");
        var isPlacable = placable.is(":checked");

        enableEditorDialog.prop("disabled", !isPlacable);
    };

})(jQuery);