(function ($) {
    
    $("input[type='checkbox'][name$='TaxonomyFieldSettings.Autocomplete']").change(function (e) {
        toggleAllowCustomTermsOption($(this).parents("fieldset.autocomplete-settings:first"), true);
    });

    $(function () {
        $("fieldset.autocomplete-settings").each(function () {
            toggleAllowCustomTermsOption($(this), false);
        });
    });

    var toggleAllowCustomTermsOption = function ($wrapper) {
        var $autocompleteCheckbox = $("input[name$='Autocomplete']", $wrapper);
        var $allowCustomTermsCheckbox = $(".allow-custom-terms-wrapper", $wrapper);
        var enableAutocomplete = $autocompleteCheckbox.is(":checked");

        if (enableAutocomplete) {
            $allowCustomTermsCheckbox.removeAttr("disabled");
        }
        else {
            $allowCustomTermsCheckbox.attr("disabled", true);
        }
        
    };

})(jQuery);