function displayDateTimeEditorOptions() {
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

    // show/hide unit selectors
    $("#value-type-date:checked").each(function () {
        $("#value-unit, [for='value-unit']").hide();
        $("#min-unit, [for='min-unit']").hide();
        $("#max-unit, [for='max-unit']").hide();
    });

    $("#value-type-timespan:checked").each(function () {
        $("#value-unit, [for='value-unit']").show();
        $("#min-unit, [for='min-unit']").show();
        $("#max-unit, [for='max-unit']").show();
    });

};

jQuery(function ($) {
    displayDateTimeEditorOptions();
    $('#operator').change(displayDateTimeEditorOptions);
    $('#value-type-date').click(displayDateTimeEditorOptions);
    $('#value-type-timespan').click(displayDateTimeEditorOptions);
});
