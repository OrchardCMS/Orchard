jQuery.fn.extend({
    slugify: function(options) {
        //todo: (heskew) need messaging system
        if (!options.target || !options.url)
            return;

        var args = {
            "contentType": options.contentType,
            __RequestVerificationToken: $("input[name=__RequestVerificationToken]").val()
        };
        args[$(this).attr("name")] = $(this).val();

        jQuery.post(
            options.url,
            args,
            function(data) {
                options.target.val(data);
            },
            "json"
        );
    }
});