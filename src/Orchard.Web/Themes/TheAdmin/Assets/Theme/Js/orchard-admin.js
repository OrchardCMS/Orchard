(function ($) {

    $(document).ready(function () {
        App.init();

        // Pagers.
        $('select.pager.selector').change(function () {
            window.location = $(this).attr("disabled", true).val();
        });

        // Auto-bulk actions.
        $(".bulk-actions-auto select").change(function () {
            $(this).closest("form").find(".apply-bulk-actions-auto:first").click();
        });

        // RemoveUrl and UnsafeUrl.
        $("body").on("click", "[itemprop~='RemoveUrl']", function () {
            // Don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled in base.js.
            if ($(this).filter("[itemprop~='UnsafeUrl']").length === 1) {
                return false;
            }

            // Use a custom message if its set in data-message.
            var dataMessage = $(this).data('message');
            if (dataMessage === undefined) {
                dataMessage = $("[data-default-remove-confirmation-prompt]").data("default-remove-confirmation-prompt");
            }

            return confirm(dataMessage);
        });

        $(".check-all").change(function () {
            $("input[type=checkbox]:not(:disabled)").prop('checked', $(this).prop("checked"));
        });

        // Handle keypress events in bulk action fieldsets that are part of a single form.
        // This will make sure the expected action executes when pressing "enter" on a text field.
        $("form .bulk-actions").on("keypress", "input[type='text']", function (e) {
            if (e.which !== 13)
                return;

            var sender = $(this);
            var fieldset = sender.closest("fieldset.bulk-actions");
            var submitButton = fieldset.find("button[type='submit']");

            if (submitButton.length === 0)
                return;

            e.preventDefault();
            submitButton.click();
        });
    });
})(jQuery);