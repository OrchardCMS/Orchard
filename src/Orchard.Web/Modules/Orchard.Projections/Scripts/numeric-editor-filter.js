function displayNumericEditorOptions() {
    // show/hide Min/Max fields
    $("#operator option:selected").each(function () {
        var val = $(this).val();
        if (val == 'Between' || val == 'NotBetween') {
            $('#fieldset-single').hide();
            $('#fieldset-min').show();
            $('#fieldset-max').show();
        }
        else {
            $('#fieldset-single').show();
            $('#fieldset-min').hide();
            $('#fieldset-max').hide();
        }
    });
};

jQuery(function ($) {
    displayNumericEditorOptions();
    $('#operator').change(displayNumericEditorOptions);
});
