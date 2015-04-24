(function ($) {

    $.validator.addMethod("optionrequired", function (value, element, param) {
        var isValid = true;

        if ($(element).is("input")) {
            var parent = $(element).closest("ol");

            isValid = parent.find("input:checked").length > 0;
            parent.toggleClass("input-validation-error", !isValid);
        }
        else if ($(element).is("select")) {
            var v = $(element).val();
            isValid = !!v && v.length > 0;
        }

        return isValid;
    }, "An option is required");

    $.validator.unobtrusive.adapters.addBool("mandatory", "required");
    $.validator.unobtrusive.adapters.addBool("optionrequired");
}(jQuery));