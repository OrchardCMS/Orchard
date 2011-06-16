// UnsafeUrl links -> form POST
//todo: need some real microdata support eventually (incl. revisiting usage of data-* attributes)
jQuery(function ($) {
    var magicToken = $("input[name=__RequestVerificationToken]").first();
    if (!magicToken) { return; } // no sense in continuing if form POSTS will fail
    $("a[itemprop~=UnsafeUrl]").each(function () {
        var _this = $(this);
        var hrefParts = _this.attr("href").split("?");
        var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
        form.append(magicToken.clone());
        if (hrefParts.length > 1) {
            var queryParts = hrefParts[1].split("&");
            for (var i = 0; i < queryParts.length; i++) {
                var queryPartKVP = queryParts[i].split("=");
                //trusting hrefs in the page here
                form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
            }
        }
        form.css({ "position": "absolute", "left": "-9999em" });
        $("body").append(form);
        _this.click(function (e) { form.submit(); return false; });
    });
});

