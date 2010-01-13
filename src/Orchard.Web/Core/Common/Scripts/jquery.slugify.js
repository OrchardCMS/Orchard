jQuery.fn.extend({
    slugify: function(options) {
        //todo: (heskew) need messaging system
        if (!options.target || !options.url) return;
        jQuery.post(options.url, { value: $(this).val(), __RequestVerificationToken: $("input[name=__RequestVerificationToken]").val() }, function(data) {
            options.target.val(data);
        }, "json");
    }
});